using Clcrutch.Extensions.DependencyInjection.OperatingSystemCheckers;

namespace Clcrutch.Extensions.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OperatingSystemRequiredAttribute : Attribute
    {
        public Type[] OperatingSystemCheckerTypes { get; }

        public OperatingSystemRequiredAttribute(OperatingSystems operatingSystem)
        {
            var operatingSystemCheckerTypes = new List<Type>();

            if ((operatingSystem & OperatingSystems.Windows) == OperatingSystems.Windows)
            {
                operatingSystemCheckerTypes.Add(typeof(WindowsOperatingSystemChecker));
            }

            if ((operatingSystem & OperatingSystems.Linux) == OperatingSystems.Linux)
            {
                operatingSystemCheckerTypes.Add(typeof(LinuxOperatingSystemChecker));
            }

            OperatingSystemCheckerTypes = operatingSystemCheckerTypes.ToArray();
        }

        public OperatingSystemRequiredAttribute(params Type[] operatingSystemCheckerType)
        {
            OperatingSystemCheckerTypes = operatingSystemCheckerType;
        }
    }
}