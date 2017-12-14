using Moq;
using Neo.Gui.ViewModels.Tests.AutoMock;

namespace Neo.Gui.ViewModels.Tests
{
    public abstract class TestBase
    {
        private MockRepository mockRepository;

        public IAutoMockContainer AutoMockContainer { get; private set; }

        public TestBase()
        {
            this.mockRepository = new MockRepository(MockBehavior.Loose);
            this.AutoMockContainer = new UnityAutoMockContainer(this.mockRepository);
        }

        public void VerifyAll()
        {
            this.mockRepository.VerifyAll();
        }
    }
}
