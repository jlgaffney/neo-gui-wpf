using System.Reflection;
using Autofac;
using Module = Autofac.Module;

namespace Neo.Gui.Cross.ViewModels
{
    public class ViewModelModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            RegisterViewModels(builder);
        }

        private static void RegisterViewModels(ContainerBuilder builder)
        {
            var assemblyTypes = Assembly.GetAssembly(typeof(ViewModelModule)).GetTypes();

            foreach (var type in assemblyTypes)
            {
                if (!type.IsSubclassOf(typeof(ViewModelBase)))
                {
                    continue;
                }

                builder.RegisterType(type);
            }
        }
    }
}
