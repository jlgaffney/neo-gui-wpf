using System;
using System.Collections;
using System.Collections.Generic;
using Autofac;
using Neo.UI.Accounts;
using Neo.UI.Assets;
using Neo.UI.Base.MVVM;
using Neo.UI.Home;
using Neo.UI.MarkupExtensions;
using Neo.UI.Updater;

namespace Neo.UI
{
    public class ViewModelsRegistrationModule : Module
    {
        private const string ViewModelTypeNameSuffix = "ViewModel";

        protected override void Load(ContainerBuilder builder)
        {
            /*builder
                .RegisterType<UpdateViewModel>()
                .As<ViewModelBase>()
                .WithParameter("ViewModel", "UpdateViewModel");
            builder
                .RegisterType<HomeViewModel>()
                .As<ViewModelBase>()
                .WithParameter("ViewModel", "HomeViewModel");
            builder
                .RegisterType<CreateMultiSigContractViewModel>()
                .As<ViewModelBase>()
                .WithParameter("ViewModel", "CreateMultiSigContractViewModel");
            builder
                .RegisterType<AssetDistributionViewModel>()
                .As<ViewModelBase>()
                .WithParameter("ViewModel", "AssetDistributionViewModel");*/

            var viewModelTypes = GetLoadedViewModelTypes();

            foreach (var viewModelType in viewModelTypes)
            {
                builder
                    .RegisterType(viewModelType)
                    .As<ViewModelBase>()
                    .WithParameter(nameof(DataContextBindingExtension.ViewModel), viewModelType);
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