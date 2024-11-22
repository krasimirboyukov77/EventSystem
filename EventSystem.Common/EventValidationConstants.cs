using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSystem.Common
{
    public class EventValidationConstants
    {
        public const string EventDateFormat = "dd.MM.yyyy HH:mm";

        public const int EventNameMinLength = 2;
        public const int EventNameMaxLength = 256;

        public const int EventDescriptionMinLength = 1;
        public const int EventDescriptionMaxLength = 500;

        public const int EventLocationMinLength = 2;
        public const int EventLocationMaxLength = 256;
    }
}
