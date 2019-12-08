using Moq;
using System;
using Xunit;

namespace Upstream.Testing.Tests
{
    public class TestBaseTests
    {
        [Fact]
        public void VerifyAll()
        {
            var goodTest = new SampleTest(true);
            goodTest.Success();
            goodTest.Dispose();

            var wrongInputTest = new SampleTest(true);
            wrongInputTest.WrongInput();
            var wrongInputException = Assert.ThrowsAny<Exception>(() => wrongInputTest.Dispose());
            Assert.IsType<MockException>(wrongInputException.InnerException);

            var notInvokedTest = new SampleTest(true);
            notInvokedTest.MockedMethodNotInvoked();
            var notInvokedException = Assert.ThrowsAny<Exception>(() => notInvokedTest.Dispose());
            Assert.IsType<MockException>(notInvokedException.InnerException);
        }

        [Fact]
        public void Verify_only_verifiable()
        {
            var goodTest = new SampleTest(verifyAll: false);
            goodTest.Success();
            goodTest.Dispose();

            var wrongInputTest = new SampleTest(verifyAll: false);
            wrongInputTest.WrongInput();
            wrongInputTest.Dispose(); // no exception thrown because mocked method is not verifiable

            var notInvokedTest = new SampleTest(verifyAll: false);
            notInvokedTest.MockedMethodNotInvoked();
            wrongInputTest.Dispose(); // no exception thrown because mocked method is not verifiable
        }

        public interface IWidget
        {
            string GetDescription(string name);
        }

        public class Widget : IWidget
        {
            public string GetDescription(string name) => throw new NotImplementedException();
        }

        public class WidgetService
        {
            private readonly IWidget _widget;

            public WidgetService(IWidget widget)
            {
                _widget = widget;
            }

            public string GetWidgetDescription(string name) => _widget.GetDescription(name);
        }

        private class SampleTest : TestBase<WidgetService>
        {
            private readonly Mock<IWidget> _widgetMock = new Mock<IWidget>();

            public SampleTest(bool verifyAll)
            {
                VerifyAll = verifyAll;
            }

            protected override bool VerifyAll { get; }

            protected override WidgetService CreateTestClass() => new WidgetService(_widgetMock.Object);

            public void Success()
            {
                _widgetMock.Setup(m => m.GetDescription("foo")).Returns("bar");

                var service = CreateTestClass();

                Assert.Equal("bar", service.GetWidgetDescription("foo"));
            }

            public void WrongInput()
            {
                _widgetMock.Setup(m => m.GetDescription("foo")).Returns("bar");

                var service = CreateTestClass();

                service.GetWidgetDescription("WUMBO"); // input not mocked
            }

            public void MockedMethodNotInvoked()
            {
                _widgetMock.Setup(m => m.GetDescription("foo")).Returns("bar");

                var service = CreateTestClass();
            }
        }
    }
}
