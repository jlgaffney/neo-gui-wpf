using System;
using System.Collections.Generic;
using Autofac;
using Neo.Gui.Wpf.MVVM;

namespace Neo.Gui.Wpf.RegistrationModules
{
    public class ViewModelsRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var viewModelTypes = GetLoadedViewModelTypes();

            foreach (var viewModelType in viewModelTypes)
            {
                builder.RegisterType(viewModelType);
            }

            base.Load(builder);
        }

        private static IEnumerable<Type> GetLoadedViewModelTypes()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetExportedTypes())
                {
                    // Check if type derives from ViewModelBase
                    if (!type.IsSubclassOf(typeof(ViewModelBase))) continue;

                    // Found view model type
                    yield return type;
                }
            }
        }
    }
}