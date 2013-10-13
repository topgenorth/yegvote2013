require 'albacore'
require 'fileutils'
require 'httpclient'
require 'rubygems'
require 'rexml/document'

# utility functions
def set_env(key, value)
  if false == value
    value = 'FALSE'
  elsif true == value
    value = 'TRUE'
  end
  ENV[key.to_s.upcase] = value
end

def get_env(key)
  val = ENV[key.to_s.upcase]
  if 'FALSE' == val
    val = false
  elsif 'TRUE' == val
    val = true
  end
  val
end

def write_line(key, file)
  k = key.to_s.upcase
  v = get_env(key)
  file.puts "#{k} = #{v}"
end

if File.exists? ('.rakeenv')
  load '.rakeenv'
else
  puts "WARNING: no .rakeenv found. If you didn't set everything in your ENV, you will be unhappy."
end

@root_dir = get_env(:root_dir)
@android = get_env(:android_home) + "/tools/android"
@zipalign = get_env(:android_home) + "/tools/zipalign"

### TASKS
task :require_environment do
  env = get_env(:environment)
  if env.nil? || 0 == env.length
    raise Exception.new("No environment specified. You need to specify an environment")
  end
end

task :default => [:require_environment, :clean, :build, :sign]

desc "Removes build artifacts"
task :clean do
  old_apk = get_env(:android_final_apk)
  directories_to_delete = [
      "#{@root_dir}/YegVote2013.Android/bin",
      "#{@root_dir}/YegVote2013.Android/obj",
      "#{@root_dir}/#{old_apk}"
  ]

  directories_to_delete.each { |x|
    rm_rf x
  }

end

desc "Compiles the project."
xbuild :build => [:write_version] do |msb|
  msb.solution = get_env(:project_file)
  msb.properties = {:configuration => :release}
  msb.targets [:Clean, :Build, :SignAndroidPackage]
end

task :bump_version do
  revision = 0
  revision_file = get_env(:revision_number_file)

  if File.exists?(revision_file)
    File.open(revision_file) { |f| revision = f.readline }
  end

  revision = revision.to_i + 1
  File.open(revision_file, 'w') { |f| f.write(revision) }

  version_number = "#{get_env(:version_major)}.#{get_env(:version_minor)}.#{get_env(:version_build)}.#{revision}"

  puts "Set the version number #{version_number}"
  set_env(:revision_number, revision.to_s)
  set_env(:version_number, version_number)
end

task :write_version => [:bump_version] do

  # First update the .NET build numbers
  shared_version_info = get_env(:version_info_file)
  rm_rf shared_version_info if File.exists?(shared_version_info)

  version_number = get_env(:version_number)
  File.open(shared_version_info, 'w') { |f|
    f.puts("using System.Reflection;")
    f.puts('[assembly: AssemblyVersion("' + version_number +'")]')
    f.puts('[assembly: AssemblyFileVersion("' + version_number +'")]')
  }

  # Now we update the Android manifest.
  android_manifest = get_env(:android_manifest)
  doc = REXML::Document.new(File.new(android_manifest))
  root = doc.root
  root.attributes["android:versionCode"] = get_env(:revision_number)
  root.attributes["android:versionName"] = get_env(:version_number)

  file = File.open(android_manifest, "w")
  formatter = REXML::Formatters::Default.new
  formatter.write(doc, file)
  file.close

  # State & commit the version files to git
  `git add #{shared_version_info}`
  `git add #{android_manifest}`
  `git commit -m "Updating version numbers to #{version_number}."`

  puts "==> Set versionCode=#{get_env(:revision_number)}, versionName=#{get_env(:version_number)}"
end

desc "Signs and zip aligns the APK."
task :sign do
  store_pass = get_env(:android_keystore_pass)
  keystore = get_env(:android_keystore)
  keystore_alias = get_env(:android_keystore_alias)
  signed_apk = get_env(:signed_apk)
  input_apk = get_env(:input_apk)

  sh "jarsigner", "-verbose", "-sigalg", "MD5withRSA", "-digestalg", "SHA1", "-keystore", keystore, "-storepass", store_pass, "-signedjar", signed_apk, input_apk, keystore_alias



  final_apk = "#{@root_dir}/#{get_env(:android_final_apk)}"
  sh @zipalign, "-f", "-v", "4", signed_apk, final_apk
  puts "==> APK for distribution: #{final_apk}"
end

desc 'Grab the commit messages and make build notes from that.'
task :release_notes => [:require_environment] do
  env = get_env(:environment)
  file_name = "/tmp/#{get_env(:app_name)}-#{env}-release-notes-current.txt"
  set_env(:release_notes_path, file_name)

  if File.exists?(file_name)
    rm_rf file_name
  end

  previous_tag = "#{env}-previous"
  current_tag = "#{env}-current"

  # this will not go well if these tags don't exist.
  if 0 == `git tag -l #{previous_tag}`.length || 0 == `git tag -l #{current_tag}`.length
    puts "* ERROR: abandoning release notes, couldn't find tags"
    return
  end

  log = `git log --pretty="* %s [%an, %h]" #{previous_tag}...#{current_tag}`
  File.open(file_name, 'w') { |f| f.write(log) }

  edit_release_notes = get_env(:edit_release_notes)
  if edit_release_notes.nil?
    edit_release_notes = false
  end

  puts "edit_release_notes = #{edit_release_notes}"
  if edit_release_notes
    puts "==> Starting up EDITOR for release notes."
    editor = get_env(:editor)
    system(editor, file_name)
  else
    puts "==> Building release notes from the git commit messages."
  end
end

desc 'Tag and push tag for current release'
task :tag_release => [:require_environment] do
  if get_env(:no_tag).nil? || !get_env(:no_tag)
    puts "==> Tagging the current build"
    env = get_env(:environment)

    # tag a specific tag so we can refer to this version
    # This generates a *lot* of tags, but some people might find it useful.
    # should add a flag & turn it off by default
    # build_number = `agvtool vers -terse`.chomp
    # date_str = Time.now.strftime('%Y-%m-%d')
    # puts `git tag -fam '' #{env}-#{date_str}-build-#{build_number} HEAD`
    # puts `git push -f origin #{env}-#{date_str}-build-#{build_number}`

    # move current -> previous
    current = `git show-ref --tags --hash --abbrev #{env}-current`.chomp
    head = `git show-ref --hash --abbrev HEAD`.chomp

    if current && current.length > 0
      if current == head
        puts "* Looks like this release was already tagged."
        return
      end

      puts `git tag -fam '' #{env}-previous #{env}-current`
      # don't push by default - again, maybe add a flag?
      #puts `git push -f origin refs/tags/#{env}-previous`
    else # there was no current - assume this is first build, tag first commit as -previous
      first_commit = `git log --pretty="%H" | tail -1`.chomp
      puts `git tag -fam '' #{env}-previous #{first_commit}`
    end

    # re-tag current
    puts `git tag -fam '' #{env}-current HEAD`
    # don't push by default - again, maybe add a flag?
    #puts `git push -f origin refs/tags/#{env}-current`
  end
end

task :release => [:require_environment, :clean, :write_version, :build, :sign, :release_notes ]