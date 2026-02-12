using FuncUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace BlobTriggerFunc.Tests
{
    public class BlobUploadedFuncTests
    {
        private readonly Mock<IEmailSender> _emailSenderMock;
        private readonly Mock<ILogger<BlobUploaded>> _loggerMock;
        private readonly Mock<IConfiguration> _configMock;
        private const string DefaultRecipient = "recipient@example.test";

        public BlobUploadedFuncTests()
        {
            _emailSenderMock = new Mock<IEmailSender>();
            _loggerMock = new Mock<ILogger<BlobUploaded>>();
            _configMock = new Mock<IConfiguration>();
            _configMock.Setup(c => c["NotificationRecipient"]).Returns(DefaultRecipient);
        }

        [Fact]
        public async Task Run_ValidBlob_SendsEmailWithCorrectContent()
        {
            // Arrange
            var fucntion = new BlobUploaded(
                _emailSenderMock.Object,
                _configMock.Object,
                _loggerMock.Object);

            var blobName = "testblob.txt";
            var blobContent = "This is a test blob content.";
            var emailBody = $"A new blob named <strong>{blobName}</strong> was uploaded.";
            using var blobStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(blobContent));

            // Act
            await fucntion.Run(blobStream, blobName, CancellationToken.None);

            // Assert
            _emailSenderMock.Verify(es => es.SendAsync(
                DefaultRecipient,
                It.Is<string>(s => s.Contains(blobName)),
                It.Is<string>(s => s.Contains(emailBody)),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
