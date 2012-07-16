using System;
using System.Linq;
using Castle.Core.Resource;
using Castle.DynamicProxy;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using DynamicInterception;
using NUnit.Framework;

namespace DynamicInterceptorFacilityTests
{
    [TestFixture]
    public class DynamicInterceptionFacilityTest
    {
        private const string Config = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
  <facilities>
    <facility type=""DynamicInterception.DynamicInterceptorFacility, DynamicInterception"">
      <add interceptor=""DynamicInterceptorFacilityTests.SampleInterceptor, DynamicInterceptorFacilityTests""
           toTypesMatching=""OneConcreteType""/> 
      <add interceptor=""DynamicInterceptorFacilityTests.SampleInterceptor, DynamicInterceptorFacilityTests""
           toTypesMatching="".*Controller""/> 
      <add interceptor=""DynamicInterceptorFacilityTests.SampleInterceptor2, DynamicInterceptorFacilityTests""
           toTypesMatching=""OtherClass""/> 
    </facility>
  </facilities>
</configuration>";

        [Test]
        public void Facility_Can_Be_Added_From_An_Xml_Configuration_File()
        {
            var container = new WindsorContainer();
            container.Install(Configuration.FromXml(new StaticContentResource(Config)));

            Assert.That(container.Kernel.GetFacilities().Any(x => x is DynamicInterceptorFacility), "Cannot find the DynamicInterceptorFacility");
        }

        [Test]
        public void Construction_Contributor_Are_Added_For_Each_Interceptor_Types_To_Match_Pair()
        {
            var container = new WindsorContainer();
            container.Install(Configuration.FromXml(new StaticContentResource(Config)));

            AssertExistsContributor(container, typeof (SampleInterceptor), "OneConcreteType");
            AssertExistsContributor(container, typeof(SampleInterceptor), ".*Controller");
            AssertExistsContributor(container, typeof(SampleInterceptor2), "OtherClass");
        }

        [Test]
        public void Type_Is_Intercepted_When_Name_Matches_Any_Of_TypesToIntercept()
        {
            var container = new WindsorContainer();
            container.Install(Configuration.FromXml(new StaticContentResource(Config)));

            container.Register(Component.For<SampleController>());

            SampleInterceptor.interceptedCalls = 0;

            var controller = container.Resolve<SampleController>();
            controller.DoSomething();

            Assert.That(SampleInterceptor.interceptedCalls, Is.EqualTo(1));
        }

        [Test]
        public void Type_Is_Not_Intercepted_When_Name_Does_Not_Match_Any_Of_TypesToIntercept()
        {
            var container = new WindsorContainer();
            container.Install(Configuration.FromXml(new StaticContentResource(Config)));

            container.Register(Component.For<OtherTypeThatDoesNotMatchAnyFilter>());

            SampleInterceptor.interceptedCalls = 0;

            var service = container.Resolve<OtherTypeThatDoesNotMatchAnyFilter>();
            service.DoSomething();

            Assert.That(SampleInterceptor.interceptedCalls, Is.EqualTo(0));
        }

        private static void AssertExistsContributor(IWindsorContainer container, Type interceptorType, string filter)
        {
            var contributor = container.Kernel.ComponentModelBuilder.Contributors
                .OfType<DynamicInterceptionConstructionContributor>()
                .FirstOrDefault(x => x.InterceptorType == interceptorType && x.TypeFilter.ToString() == filter);

            Assert.IsNotNull(contributor, "Cannot find contributor with interceptor type {0} and filter {1}", interceptorType, filter);
        }
    }

    public class SampleInterceptor : IInterceptor
    {
        public static int interceptedCalls = 0;

        public void Intercept(IInvocation invocation)
        {
            interceptedCalls++;
            invocation.Proceed();
        }
    }

    public class SampleInterceptor2 : IInterceptor
    {
        public static int interceptedCalls = 0;

        public void Intercept(IInvocation invocation)
        {
            interceptedCalls++;
            invocation.Proceed();
        }
    }

    public class SampleController
    {
        // Since we are not going to use an interface, this method must be virtual to be intercepted
        public virtual void DoSomething()
        {
        }
    }

    public class OtherTypeThatDoesNotMatchAnyFilter
    {
        public virtual void DoSomething()
        {
        }
    }

    public class OtherClass
    {
    }
}
