using Moq;
using Neo.Gui.TestHelpers.AutoMock;

namespace Neo.Gui.TestHelpers
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
