﻿using System;
using System.Linq;
using System.Threading;
using Gibraltar.Agent;
using NUnit.Framework;

namespace Loupe.Agent.Web.Module.Tests
{
    /// <summary>
    /// All messages sent in these tests have their severity set to Warning to ensure that
    /// the Log.MessageAlert event is rasied simply so we can interrogate the details we
    /// have logged to ensure they are created correctly
    /// </summary>
    [TestFixture]
    public class When_logging_details:TestBase
    {
        private ManualResetEventSlim _resetEvent;
        private LogMessageAlertEventArgs _eventArgs;

        private const string ExpectedMethodSourceInfo =
            "<MethodSourceInfo><File>app.js</File><Line>3</Line><Column>5</Column></MethodSourceInfo>";

        private const string ExpectedClientDetails =
            "<ClientDetails><Description>Firefox 37.0 32-bit on Windows 8.1 64-bit</Description><Layout>Gecko</Layout><Name>Firefox</Name><UserAgentString>Mozilla/5.0 (Windows NT 6.3; WOW64; rv:37.0) Gecko/20100101 Firefox/37.0</UserAgentString><Version>37.0</Version><OS><Architecture>64</Architecture><Family>Windows</Family><Version>8.1</Version></OS><Size><Height>873</Height><Width>1102</Width></Size></ClientDetails>";

        private const string ExpectedUserSuppliedJson = "<UserSupplied><numericValue>1</numericValue><stringValue>text value</stringValue><objectValue><childNumber>3</childNumber><childText>child text</childText></objectValue></UserSupplied>";

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Log.MessageAlert += Log_MessageAlert;
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Log.MessageAlert -= Log_MessageAlert;
        }

        [SetUp]
        public void SetUp()
        {
            _eventArgs = null;
            _resetEvent = new ManualResetEventSlim();
        }

        [Test]
        public void Should_log_expected_timestamp_and_sequence()
        {
            var currentDateTime = DateTime.Now;
            var timeStamp = new DateTimeOffset(currentDateTime, TimeZoneInfo.Local.GetUtcOffset(DateTime.Now));

            var jsonTimeStamp = timeStamp.ToString("yyyy-MM-ddTHH:mm:sszzz");

            SendRequest("{ session: { client: {description:'Firefox 37.0 32-bit on Windows 8.1 64-bit',layout:'Gecko',manufacturer:null,name:'Firefox',prerelease:null,product:null,ua:'Mozilla/5.0 (Windows NT 6.3; WOW64; rv:37.0) Gecko/20100101 Firefox/37.0',version:'37.0',os:{architecture:64,family:'Windows',version:'8.1'},size:{width:1102,height:873}}},LogMessages:[{severity: 4,category: 'Test',caption: 'test log',description: 'tests logs message',paramters: null,details: null,exception: {},methodSourceInfo: {}, timeStamp: '" + jsonTimeStamp + "', sequence: 1}]}");

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            Assert.That(loggedMessage.Details, Contains.Substring("<TimeStamp>" + timeStamp + "</TimeStamp><Sequence>1</Sequence>"));
        }

        [Test]
        public void Should_log_method_source_info_details()
        {

            SendRequest("{Session:null,LogMessages:[{severity: 4,category: 'Test',caption: 'test log',description: 'tests logs message',paramters: null,details: null,exception: {},methodSourceInfo: {file:'app.js', line: 3, column: 5}}]}");

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            Assert.That(loggedMessage.Details, Contains.Substring(ExpectedMethodSourceInfo));
        }

        [Test]
        public void Should_log_client_details()
        {
            SendRequest("{session: { client: {description:'Firefox 37.0 32-bit on Windows 8.1 64-bit',layout:'Gecko',manufacturer:null,name:'Firefox',prerelease:null,product:null,ua:'Mozilla/5.0 (Windows NT 6.3; WOW64; rv:37.0) Gecko/20100101 Firefox/37.0',version:'37.0',os:{architecture:64,family:'Windows',version:'8.1'},size:{width:1102,height:873}}},logMessages:[{severity: 4,category: 'Test',caption: 'test log',description: 'tests logs message',paramters: null,details: null,exception: {},methodSourceInfo: {file:'app.js', line: 3, column: 5}}]}");

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            Assert.That(loggedMessage.Details, Contains.Substring(ExpectedClientDetails));
        }

        [Test]
        public void Should_output_user_supplied_JSON_as_xml()
        {
            SendRequest("{Session:null,LogMessages:[{severity: 4,category: 'Test',caption: 'test log',description: 'tests logs message',paramters: null,details: \"{ numericValue: 1, stringValue: 'text value', objectValue: {childNumber: 3, childText: 'child text'}}\",exception: {},methodSourceInfo: {file:'app.js', line: 3, column: 5}}]}");

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            Assert.That(loggedMessage.Details, Contains.Substring(ExpectedUserSuppliedJson));

        }

        [Test]
        public void Should_output_user_supplied_details_as_string()
        {
            SendRequest("{Session:null,LogMessages:[{severity: 4,category: 'Test',caption: 'test log',description: 'tests logs message',paramters: null,details: 'this is user supplied details',exception: {},methodSourceInfo: {file:'app.js', line: 3, column: 5}}]}");

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            Assert.That(loggedMessage.Details, Contains.Substring("<UserSupplied>this is user supplied details</UserSupplied>"));            
        }

        [Test]
        public void Should_output_expected_details_block()
        {
            var currentDateTime = DateTime.Now;
            var timeStamp = new DateTimeOffset(currentDateTime, TimeZoneInfo.Local.GetUtcOffset(DateTime.Now));

            var jsonTimeStamp = timeStamp.ToString("yyyy-MM-ddTHH:mm:sszzz");

            SendRequest("{ session: { client: {description:'Firefox 37.0 32-bit on Windows 8.1 64-bit',layout:'Gecko',manufacturer:null,name:'Firefox',prerelease:null,product:null,ua:'Mozilla/5.0 (Windows NT 6.3; WOW64; rv:37.0) Gecko/20100101 Firefox/37.0',version:'37.0',os:{architecture:64,family:'Windows',version:'8.1'},size:{width:1102,height:873}}},LogMessages:[{severity: 4,category: 'Test',caption: 'test log',description: 'tests logs message',paramters: null,details: \"{ numericValue: 1, stringValue: 'text value', objectValue: {childNumber: 3, childText: 'child text'}}\",exception: {},methodSourceInfo: {file:'app.js', line: 3, column: 5}, timeStamp: '" + jsonTimeStamp + "', sequence: 1}]}");

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            var expectedDetailsBlock = "<Details><TimeStamp>" + timeStamp + "</TimeStamp><Sequence>1</Sequence>" +
                                       ExpectedClientDetails + ExpectedMethodSourceInfo + ExpectedUserSuppliedJson + "</Details>";

            Assert.That(loggedMessage.Details, Is.EqualTo(expectedDetailsBlock));
            
        }

        void Log_MessageAlert(object sender, LogMessageAlertEventArgs e)
        {
            _eventArgs = e;
            e.MinimumDelay = new TimeSpan(0);
            _resetEvent.Set();
        }

        private void WaitForEvent()
        {
            _resetEvent.Wait(new TimeSpan(0, 0, 0, 5));
        }
    }
}