using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Neo.Gui.TestHelpers;
using Neo.Gui.ViewModels.Accounts;
using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Services.Interfaces;
using Xunit;

namespace Neo.Gui.ViewModels.Tests.Accounts
{
    public class ImportCertificateViewModelTests: TestBase
    {
        [Fact]
        public void Ctr_CreateValidImportCertificateViewModel()
        {
            // Arrange
            // Act
            var viewModel = this.AutoMockContainer.Create<ImportCertificateViewModel>();

            //Assert
            Assert.IsType<ImportCertificateViewModel>(viewModel);
        }

        [Fact]
        public void OnLoad_StoreCerticatesAreLoaded()
        {
            // Arrange
            var firstCertificate = new X509Certificate2();
            var certificates = new List<X509Certificate2>
            {
                firstCertificate
            };

            var storeCertificateServiceMock = this.AutoMockContainer.GetMock<IStoreCertificateService>();
            storeCertificateServiceMock
                .Setup(x => x.GetStoreCertificates())
                .Returns(certificates);

            var viewModel = this.AutoMockContainer.Create<ImportCertificateViewModel>();

            // Act
            viewModel.OnLoad();

            // Assert
            Assert.Single(viewModel.Certificates);
            Assert.True(viewModel.Certificates.Single().Equals(firstCertificate));
        }

        [Fact]
        public void OkCommand_CertificateIsSelected_CertificateIsImportedAndDialogIsClosed()
        {
            // Arrange
            var closeAutoResetEvent = new AutoResetEvent(false);
            var expectedCloseEventRaised = false;
            var expectsSelectedCertificate = new X509Certificate2();

            var walletControllerMock = this.AutoMockContainer.GetMock<IWalletController>();

            var viewModel = this.AutoMockContainer.Create<ImportCertificateViewModel>();
            viewModel.SelectedCertificate = expectsSelectedCertificate;

            viewModel.Close += (sender, args) =>
            {
                expectedCloseEventRaised = true;
                closeAutoResetEvent.Set();
            };

            // Act
            viewModel.OkCommand.Execute(null);
            closeAutoResetEvent.WaitOne();

            // Assert
            walletControllerMock.Verify(x => x.ImportCertificate(expectsSelectedCertificate));
            Assert.True(expectedCloseEventRaised);
        }
    }
}
