namespace DependencyManager.Core
{
    public class UserRequiredException : Exception
    {
        public UserRequiredException()
            : base("This provider requires user requirements.  Please restart the process as user.") { }
    }
}