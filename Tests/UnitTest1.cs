using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using AmplitudeSharp;
using AmplitudeSharp.Api;
using System.Threading.Tasks;
using static AmplitudeSharp.Api.IAmplitudeApi;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            //Mock<IAmplitudeApi> api = new Mock<IAmplitudeApi>();
            //api.Setup(x => x.Identify(It.Is<AmplitudeIdentify>(e => true)))
            //    .Returns(Task.FromResult<SendResult>(SendResult.Success));
        }
    }
}
