using CodeBridge.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CodeBridge.Domain.Entities.Identity
{
    public class Saeed : BaseEntity
    {
        public Saeed()
        {
          
        }

        public string Name { get; set; }    

        public override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
