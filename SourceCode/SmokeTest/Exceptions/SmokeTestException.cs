namespace SmokeTest.Exceptions
{
  [System.Serializable]
  public class SmokeTestException : System.Exception
  {
    public SmokeTestException()
    {
    }

    public SmokeTestException(string message)
      : base(message)
    {
    }

    public SmokeTestException(string message, System.Exception inner)
      : base(message, inner)
    {
    }

    // A constructor is needed for serialization when an
    // exception propagates from a remoting server to the client.
    protected SmokeTestException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
    {
    }
  }
}
