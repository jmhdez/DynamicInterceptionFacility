using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Castle.Core;
using Castle.Core.Configuration;
using Castle.MicroKernel;
using Castle.MicroKernel.Facilities;
using Castle.MicroKernel.ModelBuilder;
using Castle.MicroKernel.Registration;

namespace DynamicInterception
{
    public class DynamicInterceptorFacility : AbstractFacility
    {
        protected override void Init()
        {
            // TODO: Handle configuration error with meaningful messages
            BuildContributors().ToList().ForEach(contributor =>
            {
                if (!Kernel.HasComponent(contributor.InterceptorType))
                    Kernel.Register(Component.For(contributor.InterceptorType));

                Kernel.ComponentModelBuilder.AddContributor(contributor);
            });
        }

        private IEnumerable<DynamicInterceptionConstructionContributor> BuildContributors()
        {
            return from child in FacilityConfig.Children
                   let typeFilter = new Regex(child.Attributes["toTypesMatching"])
                   let interceptorType = Type.GetType(child.Attributes["interceptor"], true)
                   where child.Name == "add" 
                   select new DynamicInterceptionConstructionContributor(interceptorType, typeFilter);
        }
    }

    public class DynamicInterceptionConstructionContributor : IContributeComponentModelConstruction
    {
        public Type InterceptorType { get; private set; }
        public Regex TypeFilter { get; private set; }

        public DynamicInterceptionConstructionContributor(Type interceptorType, Regex typeFilter)
        {
            InterceptorType = interceptorType;
            TypeFilter = typeFilter;
        }

        public void ProcessModel(IKernel kernel, ComponentModel model)
        {
            if (model.Services.Any(service => TypeFilter.IsMatch(service.FullName ?? "")))
                model.Interceptors.Add(new InterceptorReference(InterceptorType));
        }
    }
}
