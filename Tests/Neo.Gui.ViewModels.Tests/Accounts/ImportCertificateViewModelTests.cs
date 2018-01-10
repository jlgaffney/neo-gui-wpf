using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Neo.Gui.Base.Services.Interfaces;
using Neo.Gui.TestHelpers;
using Neo.Gui.ViewModels.Accounts;
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
    }
}
