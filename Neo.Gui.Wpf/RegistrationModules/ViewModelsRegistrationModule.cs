using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;

using GalaSoft.MvvmLight;

using Module = Autofac.Module;

namespace Neo.Gui.Wpf.RegistrationModules
{
    public class ViewModelsRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var viewModelTypes = GetViewModelTypesInAssembly();

            foreach (var viewModelType in viewModelTypes)
            {
                builder.RegisterType(viewModelType);
            }

            base.Load(builder);
        }

        private static IEnumerable<Type> GetViewModelTypesInAssembly()
        {
            var assembly = Assembly.GetAssembly(typeof(ViewModelsRegistrationModule));

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