using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AddressParser
{
    /// <summary>
    /// Contains the fields that were extracted by the <see cref="AddressParser"/> object.
    /// </summary>
    public class AddressParseResult
    {
        /// <summary>
        /// The street line.
        /// </summary>
        private string _streetLine;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressParseResult"/> class.
        /// </summary>
        /// <param name="fields">The fields that were parsed.</param>
        internal AddressParseResult(Dictionary<string, string> fields)
        {
            if (fields == null)
                throw new ArgumentNullException(nameof(fields));

            LoadPropertyFromDictionary(fields);
        }

        private void LoadPropertyFromDictionary(IDictionary<string, string> source)
        {
            var someObjectType = typeof(AddressParseResult);

            foreach (var item in source)
            {
                var prop = someObjectType.GetProperty(item.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (prop != null)
                    prop.SetValue(this, item.Value, null);
            }

        }

        /// <summary>
        /// Gets the city name.
        /// </summary>
        public string City
        {
            get;
            // ReSharper disable once UnusedAutoPropertyAccessor.Local  reflection uses this
            private set;
        }

        /// <summary>
        /// Gets the house number.
        /// </summary>
        public string Number
        {
            get;
            // ReSharper disable once UnusedAutoPropertyAccessor.Local  reflection uses this
            private set;
        }

        /// <summary>
        /// Gets the predirectional, such as "N" in "500 N Main St".
        /// </summary>
        public string Predirectional
        {
            get;
            // ReSharper disable once UnusedAutoPropertyAccessor.Local  reflection uses this
            private set;
        }

        /// <summary>
        /// Gets the postdirectional, such as "NW" in "500 Main St NW".
        /// </summary>
        public string Postdirectional
        {
            get;
            // ReSharper disable once UnusedAutoPropertyAccessor.Local  reflection uses this
            private set;
        }

        /// <summary>
        /// Gets the state or territory.
        /// </summary>
        public string State
        {
            get;
            // ReSharper disable once UnusedAutoPropertyAccessor.Local  reflection uses this
            private set;
        }

        /// <summary>
        /// Gets the name of the street, such as "Main" in "500 N Main St".
        /// </summary>
        public string Street
        {
            get;
            // ReSharper disable once UnusedAutoPropertyAccessor.Local  reflection uses this
            private set;
        }

        /// <summary>
        /// Gets the full street line, such as "500 N Main St" in "500 N Main St".
        /// This is typically constructed by combining other elements in the parsed result.
        /// However, in some special circumstances, most notably APO/FPO/DPO addresses, the
        /// street line is set directly and the other elements will be null.
        /// </summary>
        public string StreetLine
        {
            get
            {
                if (_streetLine == null)
                {
                    var streetLine = string.Join(
                        " ", Number, Predirectional, Street, Suffix, Postdirectional, SecondaryUnit, SecondaryNumber);
                    streetLine = Regex
                        .Replace(streetLine, @"\ +", " ")
                        .Trim();
                    return streetLine;
                }

                return _streetLine;
            }

            // ReSharper disable once UnusedAutoPropertyAccessor.Local  reflection uses this
            // ReSharper disable once UnusedMember.Local
            private set => _streetLine = value;
        }

        /// <summary>
        /// Gets the street suffix, such as "ST" in "500 N MAIN ST".
        /// </summary>
        public string Suffix
        {
            get;
            // ReSharper disable once UnusedAutoPropertyAccessor.Local  reflection uses this
            private set;
        }

        /// <summary>
        /// Gets the secondary unit, such as "APT" in "500 N MAIN ST APT 3".
        /// </summary>
        public string SecondaryUnit
        {
            get;
            // ReSharper disable once UnusedAutoPropertyAccessor.Local  reflection uses this
            private set;
        }

        /// <summary>
        /// Gets the secondary unit, such as "3" in "500 N MAIN ST APT 3".
        /// </summary>
        public string SecondaryNumber
        {
            get;
            // ReSharper disable once UnusedAutoPropertyAccessor.Local  reflection uses this
            private set;
        }

        /// <summary>
        /// Gets the ZIP code.
        /// </summary>
        public string Zip
        {
            get;
            // ReSharper disable once UnusedAutoPropertyAccessor.Local  reflection uses this
            private set;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{StreetLine}; {City}, {State}  {Zip}";
        }
    }
}
