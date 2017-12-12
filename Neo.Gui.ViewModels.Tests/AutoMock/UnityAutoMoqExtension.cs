using Moq;
using Unity.Builder;
using Unity.Extension;

namespace Neo.Gui.ViewModels.Tests.AutoMock
{
    public class UnityAutoMoqExtension : UnityContainerExtension
    {
        private readonly MockRepository mockRepository;
        private readonly UnityAutoMockContainer autoMockContainer;

        public UnityAutoMoqExtension(
            MockRepository mockRepository,
            UnityAutoMockContainer autoMockContainer)
        {
            this.mockRepository = mockRepository;
            this.autoMockContainer = autoMockContainer;
        }

        protected override void Initialize()
        {
            Context.Strategies.Add(
                new UnityAutoMoqBuilderStrategy(this.mockRepository, this.autoMockContainer),
                UnityBuildStage.PreCreation);
        }
    }
}
