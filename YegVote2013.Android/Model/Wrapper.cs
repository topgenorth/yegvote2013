namespace YegVote2013.Droid.Model
{
    using Java.Lang;

    /// <summary>
    ///   This class is used to wrap a .NET managed type inside a Java object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Wrapper<T> : Object
    {
        public Wrapper(T value)
        {
            Value = value;
        }

        public T Value { get; private set; }
    }
}
