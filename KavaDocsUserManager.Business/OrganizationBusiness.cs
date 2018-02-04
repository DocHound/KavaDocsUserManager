using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using KavaDocsUserManager.Business.Configuration;
using KavaDocsUserManager.Business.Models;
using Microsoft.EntityFrameworkCore;
using Westwind.Data.EfCore;
using Westwind.Utilities;

namespace KavaDocsUserManager.Business
{

    public class OrganizationBusiness : EntityFrameworkBusinessObject<KavaDocsContext, Organization>
    {
        private KavaDocsConfiguration Configuration;

        public OrganizationBusiness(KavaDocsContext context, KavaDocsConfiguration config) : base(context)
        {
            Context = context;
            Configuration = config;
        }

        public bool CreateOrganization(Organization organization)
        {
            Context.Organizations.Add(organization);

            if (!Validate(organization))
                return false;

            return Save();
        }
    
        
        public bool AddRepositoryToOrganization(Guid organizationId, Guid repositoryId)
        {
            if (Context.OrganizationRepositories
                .Any(or => or.OrganizationId == organizationId && or.RepositoryId == repositoryId))
                return true; // already exists
                
            var organization = Context.Organizations.FirstOrDefault(o => o.Id == organizationId);
            if (organization == null)
            {
                SetError("Invalid organization to add repository to.");
                return false;
            }

            var repository = Context.Repositories.FirstOrDefault(o => o.Id == repositoryId);
            if (repository == null)
            {
                SetError("Invalid repositry specified.");
                return false;
            }

            var map = new OrganizationRepository
            {
                OrganizationId = organizationId,
                RepositoryId = repositoryId
            };
            organization.Repositories.Add(map);

            return Save();
        }


        protected override bool OnValidate(Organization entity)
        {
            ValidationErrors.Clear();

            if (string.IsNullOrEmpty(entity.Title))
                ValidationErrors.Add("Title can't be empty");


            return ValidationErrors.Count > 0;
        }


    }
}
