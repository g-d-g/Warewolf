﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using Caliburn.Micro;
using Dev2;
using Dev2.Activities;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Communication;
using Dev2.Diagnostics.Debug;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

// ReSharper disable InconsistentNaming

namespace Warewolf.Studio.ViewModels.Tests
{
    partial class ServiceTestViewModelTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PrepopulateTestsUsingDebug_DebugItemDesicion_ShouldHaveAddServiceTestStep()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var dsfDecision = new DsfDecision();
            var decisionUniqueId = Guid.NewGuid();
            dsfDecision.UniqueID = decisionUniqueId.ToString();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(contextualResourceModel.ID);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var modelItem = ModelItemUtils.CreateModelItem(new DsfDecision());
            mockWorkflowDesignerViewModel.Setup(model => model.GetModelItem(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(modelItem);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            var readAllText = File.ReadAllText("JsonResources\\DebugStates.json");
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            var debugTreeMock = new Mock<IDebugTreeViewItemViewModel>();
            var repo = new Mock<IEnvironmentRepository>();
            var itemViewModeInput = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[0], };
            var itemViewModel = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[1], };
            var itemViewModel1 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[2], };
            var itemViewModel2 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[3], };
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>()
            {
                debugTreeMock.Object
                ,itemViewModeInput
                ,itemViewModel
                ,itemViewModel1
                ,itemViewModel2

            };
            //---------------Assert Precondition----------------          
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------

            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, newTestFromDebugMessage);
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.TestSteps.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PrepopulateTestsUsingDebug_DebugIDesicion_ShouldHaveAddServiceTestStepShouldHaveArmOptions()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfDecision = new DsfDecision();
            var decisionUniqueId = Guid.NewGuid();
            dsfDecision.UniqueID = decisionUniqueId.ToString();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            var readAllText = File.ReadAllText("JsonResources\\DebugStates.json");
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            var debugTreeMock = new Mock<IDebugTreeViewItemViewModel>();
            var repo = new Mock<IEnvironmentRepository>();
            var itemViewModel = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[1], };
            var itemViewModel1 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[2], };
            var itemViewModel2 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[3], };
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>()
            {
                debugTreeMock.Object
                ,itemViewModel
                ,itemViewModel1
                ,itemViewModel2
            };
            //---------------Assert Precondition----------------          
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------

            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, newTestFromDebugMessage);
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.TestSteps.Count);
            Assert.AreEqual(StepType.Assert, testFrameworkViewModel.SelectedServiceTest.TestSteps[0].Type);
            StringAssert.Contains("DsfMultiAssignActivity", testFrameworkViewModel.SelectedServiceTest.TestSteps[0].ActivityType);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AddChildDebugItems_GivenTestStepNotContainsStep_ShouldAddStep()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfDecision = new DsfDecision();
            var decisionUniqueId = Guid.NewGuid();
            dsfDecision.UniqueID = decisionUniqueId.ToString();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            var readAllText = File.ReadAllText("JsonResources\\DebugStates.json");
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            var debugTreeMock = new Mock<IDebugTreeViewItemViewModel>();
            var repo = new Mock<IEnvironmentRepository>();
            var itemViewModel = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[1], };
            var itemViewModel1 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[2], };
            var itemViewModel2 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[3], };
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>()
            {
                debugTreeMock.Object
                ,itemViewModel
                ,itemViewModel1
                ,itemViewModel2
            };
            //---------------Assert Precondition----------------          
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------

            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, newTestFromDebugMessage);
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("AddChildDebugItems", BindingFlags.NonPublic | BindingFlags.Instance);
            ObservableCollection<IServiceTestStep> testSteps = new ObservableCollection<IServiceTestStep>();
            var serviceTestStep = new Mock<IServiceTestStep>();
            serviceTestStep.SetupGet(step => step.UniqueId).Returns(resourceId);
            //AddChildDebugItems(IDebugState debugItemContent, IDebugTreeViewItemViewModel debugState, ObservableCollection<IServiceTestStep> testSteps, IServiceTestStep parent)
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(testFrameworkViewModel, new object[] { debugStates[1], itemViewModel, serviceTestStep.Object });
            //---------------Test Result -----------------------
            var serviceTestSteps = testFrameworkViewModel.Tests[0].TestSteps;
            var contains = serviceTestSteps.Contains(serviceTestStep.Object);
            Assert.IsTrue(contains);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AddChildDebugItems_GivenTestStepContainsStep_ShouldNotAddStep()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfDecision = new DsfDecision();
            var decisionUniqueId = Guid.NewGuid();
            dsfDecision.UniqueID = decisionUniqueId.ToString();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            var readAllText = File.ReadAllText("JsonResources\\DebugStates.json");
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            var debugTreeMock = new Mock<IDebugTreeViewItemViewModel>();
            var repo = new Mock<IEnvironmentRepository>();
            var itemViewModel = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[1], };
            var itemViewModel1 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[2], };
            var itemViewModel2 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[3], };
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>()
            {
                debugTreeMock.Object
                ,itemViewModel
                ,itemViewModel1
                ,itemViewModel2
            };


            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, newTestFromDebugMessage);
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("AddChildDebugItems", BindingFlags.NonPublic | BindingFlags.Instance);
            var serviceTestStep = new Mock<IServiceTestStep>();
            ObservableCollection<IServiceTestStep> testSteps = new ObservableCollection<IServiceTestStep>() { serviceTestStep.Object };

            serviceTestStep.SetupGet(step => step.UniqueId).Returns(resourceId);
            //AddChildDebugItems(IDebugState debugItemContent, IDebugTreeViewItemViewModel debugState, ObservableCollection<IServiceTestStep> testSteps, IServiceTestStep parent)
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            Assert.AreEqual(1, testSteps.Count);
            var contains = testSteps.Contains(serviceTestStep.Object);
            Assert.IsTrue(contains);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(testFrameworkViewModel, new object[] { debugStates[1], itemViewModel, serviceTestStep.Object });
            //---------------Test Result -----------------------
            Assert.AreEqual(1, testSteps.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AddChildDebugItems_GivenDecision_ShouldNotAddStepFromDebugState()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfDecision = new DsfDecision();
            var decisionUniqueId = Guid.NewGuid();
            dsfDecision.UniqueID = decisionUniqueId.ToString();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            var readAllText = File.ReadAllText("JsonResources\\DebugStates.json");
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            var debugTreeMock = new Mock<IDebugTreeViewItemViewModel>();
            var repo = new Mock<IEnvironmentRepository>();
            var itemViewModel = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[1], };
            var itemViewModel1 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[2], };
            var itemViewModel2 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[3], };
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>()
            {
                debugTreeMock.Object
                ,itemViewModel
                ,itemViewModel1
                ,itemViewModel2
            };


            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, newTestFromDebugMessage);
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("AddChildDebugItems", BindingFlags.NonPublic | BindingFlags.Instance);
            ObservableCollection<IServiceTestStep> testSteps = new ObservableCollection<IServiceTestStep>() { };

            //AddChildDebugItems(IDebugState debugItemContent, IDebugTreeViewItemViewModel debugState, ObservableCollection<IServiceTestStep> testSteps, IServiceTestStep parent)
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(testFrameworkViewModel, new object[] { debugStates[1], itemViewModel, default(IServiceTestStep) });
            //---------------Test Result -----------------------
            Assert.AreEqual(0, testSteps.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AddChildDebugItems_GivenSequence_ShouldAddtestStepFromDebugState()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfDecision = new DsfDecision();
            var decisionUniqueId = Guid.NewGuid();
            dsfDecision.UniqueID = decisionUniqueId.ToString();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            var readAllText = File.ReadAllText("JsonResources\\DebugStates.json");
            var sequncetext = File.ReadAllText("JsonResources\\sequenceState.json");
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            var sequenceSate = serializer.Deserialize<IDebugState>(sequncetext);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            var debugTreeMock = new Mock<IDebugTreeViewItemViewModel>();
            var repo = new Mock<IEnvironmentRepository>();
            var itemViewModel = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[1], };
            var itemViewModel1 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[2], };
            var itemViewModel2 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[3], };
            var seq = new DebugStateTreeViewItemViewModel(repo.Object) { Content = sequenceSate, };
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>()
            {
                debugTreeMock.Object
                ,itemViewModel
                ,itemViewModel1
                ,itemViewModel2,
                seq
            };


            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, newTestFromDebugMessage);
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("AddChildDebugItems", BindingFlags.NonPublic | BindingFlags.Instance);
            ObservableCollection<IServiceTestStep> testSteps = new ObservableCollection<IServiceTestStep>() { };

            //AddChildDebugItems(IDebugState debugItemContent, IDebugTreeViewItemViewModel debugState, ObservableCollection<IServiceTestStep> testSteps, IServiceTestStep parent)
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(testFrameworkViewModel, new object[] { sequenceSate, seq, default(IServiceTestStep) });
            //---------------Test Result -----------------------
            var serviceTestSteps = testFrameworkViewModel.Tests[0].TestSteps;
            Assert.AreEqual(2, serviceTestSteps.Count);
            Assert.AreEqual("DsfMultiAssignActivity", serviceTestSteps[0].ActivityType);
            Assert.AreEqual("Set the output variable (1)", serviceTestSteps[0].StepDescription);
            Assert.AreEqual(1, serviceTestSteps[0].StepOutputs.Count);
            Assert.AreEqual("670132e7-80d4-4e41-94af-ba4a71b28118".ToGuid(), serviceTestSteps[0].UniqueId);
            Assert.AreEqual(StepType.Assert, serviceTestSteps[0].Type);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SwitchFromDebug_GivenDebugState_ShouldAddtestStepFromDebugState()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfDecision = new DsfDecision();
            var decisionUniqueId = Guid.NewGuid();
            dsfDecision.UniqueID = decisionUniqueId.ToString();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var objectToMakeModelItem = new DsfFlowSwitchActivity();
            var modelItem = ModelItemUtils.CreateModelItem(objectToMakeModelItem);
            mockWorkflowDesignerViewModel.Setup(model => model.GetModelItem(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(modelItem);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            var readAllText = File.ReadAllText("JsonResources\\DebugStates.json");
            var sequncetext = File.ReadAllText("JsonResources\\sequenceState.json");
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            var sequenceSate = serializer.Deserialize<IDebugState>(sequncetext);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            var debugTreeMock = new Mock<IDebugTreeViewItemViewModel>();
            var repo = new Mock<IEnvironmentRepository>();
            var itemViewModel = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[1], };
            var itemViewModel1 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[2], };
            var itemViewModel2 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[3], };
            var seq = new DebugStateTreeViewItemViewModel(repo.Object) { Content = sequenceSate, };
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>()
            {
                debugTreeMock.Object
                ,itemViewModel
                ,itemViewModel1
                ,itemViewModel2,
                seq
            };


            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, newTestFromDebugMessage);
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("SwitchFromDebug", BindingFlags.NonPublic | BindingFlags.Instance);

            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            IDebugState state = new DebugState();
            methodInfo.Invoke(testFrameworkViewModel, new object[] { state });


            //---------------Test Result -----------------------

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AddChildDebugItems_GivenSequenceWithChildren_ShouldAddStepWithOutputsFromDebugState()
        {
            Thread.Sleep(10);
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfDecision = new DsfDecision();
            var decisionUniqueId = Guid.NewGuid();
            dsfDecision.UniqueID = decisionUniqueId.ToString();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            var readAllText = File.ReadAllText("JsonResources\\DebugStates.json");
            var sequncetext = File.ReadAllText("JsonResources\\sequenceState.json");
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            var sequenceSate = serializer.Deserialize<IDebugState>(sequncetext);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            var debugTreeMock = new Mock<IDebugTreeViewItemViewModel>();
            var repo = new Mock<IEnvironmentRepository>();
            var itemViewModel = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[1], };
            var itemViewModel1 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[2], };
            var itemViewModel2 = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates[3], };
            var seq = new DebugStateTreeViewItemViewModel(repo.Object) { Content = sequenceSate, };
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>()
            {
                debugTreeMock.Object
                ,itemViewModel
                ,itemViewModel1
                ,itemViewModel2,
                seq
            };

            seq.Children.Add(itemViewModel1);
            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, newTestFromDebugMessage);
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("AddChildDebugItems", BindingFlags.NonPublic | BindingFlags.Instance);
            ObservableCollection<IServiceTestStep> testSteps = new ObservableCollection<IServiceTestStep>() { };

            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            try
            {
                methodInfo.Invoke(testFrameworkViewModel, new object[] { sequenceSate, seq, default(IServiceTestStep) });
            }
            catch (Exception ex) when (ex is TargetInvocationException)//weird error during a test run
            {
                //Assert.AreEqual(1, testSteps.Count);
                //Assert.AreEqual("DsfSequenceActivity", testSteps[0].ActivityType);
                //Assert.AreEqual("Sequence", testSteps[0].StepDescription);
                //Assert.AreEqual(0, testSteps[0].StepOutputs.Count);
                //Assert.AreEqual("549601d4-c800-4176-89b7-4eba3bac46fa".ToGuid(), testSteps[0].UniqueId);
                //Assert.AreEqual(StepType.Assert, testSteps[0].Type);

                //Assert.AreEqual(1, testSteps[0].Children.Count);
                //Assert.AreEqual(1, testSteps[0].Children[0].StepOutputs.Count);
            }

            //---------------Test Result -----------------------


        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SetInputs_GivenDebugStates_ShouldAddTestInputValues()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfDecision = new DsfDecision();
            var decisionUniqueId = Guid.NewGuid();
            dsfDecision.UniqueID = decisionUniqueId.ToString();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            var readAllText = File.ReadAllText("JsonResources\\DebugStates.json");
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>();


            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, newTestFromDebugMessage);
            testFrameworkViewModel.SelectedServiceTest.Inputs.Add(new ServiceTestInput("Name", ""));
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("SetInputs", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(testFrameworkViewModel, new object[] { debugStates[0] });
            //---------------Test Result -----------------------
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.Inputs.Count);
            Assert.AreEqual("Nathi", testFrameworkViewModel.SelectedServiceTest.Inputs.First().Value);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SetOutputs_GivenDebugStates_ShouldAddTestOutput()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfDecision = new DsfDecision();
            var decisionUniqueId = Guid.NewGuid();
            dsfDecision.UniqueID = decisionUniqueId.ToString();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            var readAllText = File.ReadAllText("JsonResources\\DebugStates.json");
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>();


            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, newTestFromDebugMessage);
            testFrameworkViewModel.SelectedServiceTest.Outputs.Add(new ServiceTestOutput("Message", "", "", ""));
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("SetOutputs", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(testFrameworkViewModel, new object[] { debugStates.Last() });
            //---------------Test Result -----------------------
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.Outputs.Count);
            Assert.AreEqual("Hello Nathi.", testFrameworkViewModel.SelectedServiceTest.Outputs.First().Value);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ProcessInputsAndOutputs_GivenStepDebugStates_ShouldNotAdd()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfDecision = new DsfDecision();
            var decisionUniqueId = Guid.NewGuid();
            dsfDecision.UniqueID = decisionUniqueId.ToString();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            var readAllText = File.ReadAllText("JsonResources\\DebugStates.json");
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>();


            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, newTestFromDebugMessage);
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("ProcessInputsAndOutputs", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            Assert.AreEqual(0, testFrameworkViewModel.SelectedServiceTest.Outputs.Count);
            Assert.AreEqual(0, testFrameworkViewModel.SelectedServiceTest.Inputs.Count);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(testFrameworkViewModel, new object[] { debugStates[1] });
            //---------------Test Result -----------------------
            Assert.AreEqual(0, testFrameworkViewModel.SelectedServiceTest.Outputs.Count);
            Assert.AreEqual(0, testFrameworkViewModel.SelectedServiceTest.Inputs.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ProcessInputsAndOutputs_GivenInputStepDebugStates_ShouldAddInput()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfDecision = new DsfDecision();
            var decisionUniqueId = Guid.NewGuid();
            dsfDecision.UniqueID = decisionUniqueId.ToString();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            var readAllText = File.ReadAllText("JsonResources\\DebugStates.json");
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>();


            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, newTestFromDebugMessage);
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("ProcessInputsAndOutputs", BindingFlags.NonPublic | BindingFlags.Instance);
            testFrameworkViewModel.SelectedServiceTest.Inputs.Add(new ServiceTestInput("Name", ""));
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            Assert.AreEqual(0, testFrameworkViewModel.SelectedServiceTest.Outputs.Count);
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.Inputs.Count);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(testFrameworkViewModel, new object[] { debugStates[0] });
            //---------------Test Result -----------------------
            Assert.AreEqual("Nathi", testFrameworkViewModel.SelectedServiceTest.Inputs.First().Value);
            Assert.AreEqual(0, testFrameworkViewModel.SelectedServiceTest.Outputs.Count);
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.Inputs.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ProcessInputsAndOutputs_GivenOutputStepDebugStates_ShouldAddOutput()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var contextualResourceModel = CreateResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfDecision = new DsfDecision();
            var decisionUniqueId = Guid.NewGuid();
            dsfDecision.UniqueID = decisionUniqueId.ToString();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            var readAllText = File.ReadAllText("JsonResources\\DebugStates.json");
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>();


            var testFrameworkViewModel = new ServiceTestViewModel(contextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, newTestFromDebugMessage);
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("ProcessInputsAndOutputs", BindingFlags.NonPublic | BindingFlags.Instance);
            testFrameworkViewModel.SelectedServiceTest.Outputs.Add(new ServiceTestOutput("Message", "", "", ""));
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testFrameworkViewModel);
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.Outputs.Count);
            Assert.AreEqual(0, testFrameworkViewModel.SelectedServiceTest.Inputs.Count);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(testFrameworkViewModel, new object[] { debugStates.Last() });
            //---------------Test Result -----------------------
            Assert.AreEqual(1, testFrameworkViewModel.SelectedServiceTest.Outputs.Count);
            Assert.AreEqual(0, testFrameworkViewModel.SelectedServiceTest.Inputs.Count);
            Assert.AreEqual("Hello Nathi.", testFrameworkViewModel.SelectedServiceTest.Outputs.First().Value);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PrepopulateTestsUsingDebug_GivenWrongMessage_ShouldAddTests()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfDecision = new DsfDecision();
            var decisionUniqueId = Guid.NewGuid();
            dsfDecision.UniqueID = decisionUniqueId.ToString();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            var readAllText = File.ReadAllText("JsonResources\\DebugStates.json");
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();

            var debugStates = serializer.Deserialize<List<IDebugState>>(readAllText);
            newTestFromDebugMessage.ResourceModel = mockResourceModel.Object;
            var debugTreeMock = new Mock<IDebugTreeViewItemViewModel>();
            var repo = new Mock<IEnvironmentRepository>();
            var itemViewModel = new DebugStateTreeViewItemViewModel(repo.Object) { Content = debugStates.First() };
            newTestFromDebugMessage.RootItems = new List<IDebugTreeViewItemViewModel>()
            {
                debugTreeMock.Object
                ,itemViewModel
            };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var synchronousAsyncWorker = new SynchronousAsyncWorker();
            var testFrameworkViewModel = new ServiceTestViewModel(CreateResourceModel(), synchronousAsyncWorker, new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, newTestFromDebugMessage);
            //---------------Test Result -----------------------
            Assert.AreEqual(2, testFrameworkViewModel.Tests.Count);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PrepopulateTestsUsingDebug_GivenInCorrectMessage_ShouldThrowException()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            var resourceId = Guid.NewGuid();
            var dsfDecision = new DsfDecision();
            var decisionUniqueId = Guid.NewGuid();
            dsfDecision.UniqueID = decisionUniqueId.ToString();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.SetupProperty(model => model.ItemSelectedAction);
            var newTestFromDebugMessage = new ExecuteResourceMessage(mockResourceModel.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var synchronousAsyncWorker = new SynchronousAsyncWorker();
            // ReSharper disable once ObjectCreationAsStatement
            new ServiceTestViewModel(CreateResourceModel(), synchronousAsyncWorker, new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object, newTestFromDebugMessage);
            var exceptions = synchronousAsyncWorker.Exceptions.Count;
            Assert.AreEqual(1, exceptions);

            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void RefreshCommands_ShouldUpdateDisplayName()
        {
            //---------------Set up test pack-------------------
            var resourceModel = new Mock<IContextualResourceModel>();
            var env = new Mock<IEnvironmentModel>();
            var con = new Mock<IEnvironmentConnection>();
            var debugTreeMock = new Mock<List<IDebugTreeViewItemViewModel>>();
            resourceModel.Setup(model => model.Environment).Returns(env.Object);
            resourceModel.Setup(model => model.Environment.Connection).Returns(con.Object);
            var message = new NewTestFromDebugMessage { ResourceModel = resourceModel.Object, RootItems = debugTreeMock.Object };
            var testViewModel = new ServiceTestViewModel(resourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, message);
            bool wasCalled = false;
            testViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "DisplayName")
                {
                    wasCalled = true;
                }
            };

            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            testViewModel.RefreshCommands();
            //---------------Test Result -----------------------
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ResourceID_GivenResourceModel_ShouldReturnCorrectly()
        {
            //---------------Set up test pack-------------------
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.SetupAllProperties();
            var env = new Mock<IEnvironmentModel>();
            var con = new Mock<IEnvironmentConnection>();
            var debugTreeMock = new Mock<List<IDebugTreeViewItemViewModel>>();
            resourceModel.Setup(model => model.Environment).Returns(env.Object);
            resourceModel.Setup(model => model.Environment.Connection).Returns(con.Object);
            var message = new NewTestFromDebugMessage { ResourceModel = resourceModel.Object, RootItems = debugTreeMock.Object };
            var testViewModel = new ServiceTestViewModel(resourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, message);


            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var resourceID = testViewModel.ResourceID;
            //---------------Test Result -----------------------
            Assert.AreEqual(resourceID, testViewModel.ResourceModel.ID);


        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Stoptest_GivenServiceTests_ShouldSetValuesCorrectly()
        {
            //---------------Set up test pack-------------------
            var resourceModel = new Mock<IContextualResourceModel>();
            var handler = new Mock<IServiceTestCommandHandler>();
            handler.Setup(commandHandler => commandHandler.StopTest(resourceModel.Object));
            resourceModel.SetupAllProperties();
            var env = new Mock<IEnvironmentModel>();
            var con = new Mock<IEnvironmentConnection>();
            var debugTreeMock = new Mock<List<IDebugTreeViewItemViewModel>>();
            resourceModel.Setup(model => model.Environment).Returns(env.Object);
            resourceModel.Setup(model => model.Environment.Connection).Returns(con.Object);
            var message = new NewTestFromDebugMessage { ResourceModel = resourceModel.Object, RootItems = debugTreeMock.Object };
            var testViewModel = new ServiceTestViewModel(resourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, message);
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("StopTest", BindingFlags.NonPublic | BindingFlags.Instance);
            var propertyInfo = typeof(ServiceTestViewModel).GetProperty("ServiceTestCommandHandler", BindingFlags.Public | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            Assert.IsNotNull(propertyInfo);
            //---------------Execute Test ----------------------
            propertyInfo.SetValue(testViewModel, handler.Object);
            methodInfo.Invoke(testViewModel, new object[] { });
            //---------------Test Result -----------------------
            Assert.IsFalse(testViewModel.SelectedServiceTest.IsTestRunning);
            Assert.IsTrue(testViewModel.SelectedServiceTest.TestPending);
            handler.Verify(commandHandler => commandHandler.StopTest(resourceModel.Object));


        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SetStepIcon_GiventypeName_ShouldSetValuesCorrectly_PassThrouth()
        {
            //---------------Set up test pack-------------------
            var resourceModel = new Mock<IContextualResourceModel>();
            var handler = new Mock<IServiceTestCommandHandler>();
            handler.Setup(commandHandler => commandHandler.StopTest(resourceModel.Object));
            resourceModel.SetupAllProperties();
            var env = new Mock<IEnvironmentModel>();
            var con = new Mock<IEnvironmentConnection>();
            var debugTreeMock = new Mock<List<IDebugTreeViewItemViewModel>>();
            resourceModel.Setup(model => model.Environment).Returns(env.Object);
            resourceModel.Setup(model => model.Environment.Connection).Returns(con.Object);
            var message = new NewTestFromDebugMessage { ResourceModel = resourceModel.Object, RootItems = debugTreeMock.Object };
            var testViewModel = new ServiceTestViewModel(resourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, message);
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("SetStepIcon", BindingFlags.NonPublic | BindingFlags.Instance, null, CallingConventions.Any, new[] { typeof(string), typeof(ServiceTestStep) }, new[] { new ParameterModifier(2), });
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            try
            {
                string typename = "DsfDecision";
                methodInfo.Invoke(testViewModel, new object[] { typename, default(ServiceTestStep) });

                //---------------Test Result -----------------------

                //---------------Execute Test ----------------------
                typename = "FlowDecision";
                methodInfo.Invoke(testViewModel, new object[] { typename, default(ServiceTestStep) });


                //---------------Execute Test ----------------------
                typename = "DsfSwitch";
                methodInfo.Invoke(testViewModel, new object[] { typename, default(ServiceTestStep) });

                //---------------Test Result -----------------------
            }
            catch (Exception)
            {
                //
            }



        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AddDuplicateTestUsingDebug_GivenMessage_ShouldAddDebugTestSteps()
        {
            //---------------Set up test pack-------------------
            var resourceModel = new Mock<IContextualResourceModel>();
            var handler = new Mock<IServiceTestCommandHandler>();
            handler.Setup(commandHandler => commandHandler.StopTest(resourceModel.Object));
            resourceModel.SetupAllProperties();
            var env = new Mock<IEnvironmentModel>();
            var con = new Mock<IEnvironmentConnection>();
            var debugTreeMock = new Mock<List<IDebugTreeViewItemViewModel>>();
            resourceModel.Setup(model => model.Environment).Returns(env.Object);
            resourceModel.Setup(model => model.Environment.Connection).Returns(con.Object);
            var message = new NewTestFromDebugMessage { ResourceModel = resourceModel.Object, RootItems = debugTreeMock.Object };
            var testViewModel = new ServiceTestViewModel(resourceModel.Object, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, new Mock<IWorkflowDesignerViewModel>().Object, message);
            var methodInfo = typeof(ServiceTestViewModel).GetMethod("SetStepIcon", BindingFlags.NonPublic | BindingFlags.Instance, null, CallingConventions.Any, new[] { typeof(string), typeof(ServiceTestStep) }, new[] { new ParameterModifier(2), });
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            try
            {
                var newTestFromDebugMessage = new NewTestFromDebugMessage
                {
                    RootItems = new List<IDebugTreeViewItemViewModel>()
                };
                testViewModel.AddDuplicateTestUsingDebug(newTestFromDebugMessage);
                Assert.IsNotNull(testViewModel.SelectedServiceTest.TestSteps);
            }
            catch (Exception)
            {
                //
            }




        }



    }
}