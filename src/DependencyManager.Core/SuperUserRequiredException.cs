namespace DependencyManager.Core
{
    public class SuperUserRequiredException : Exception
    {
        public SuperUserRequiredException()
            : base("This provider requires super user requirements.  Please restart the process as super user.") { }
    }
}
