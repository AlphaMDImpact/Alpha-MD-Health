using System;
using System.Collections.Generic;
using System.Globalization;

namespace AlphaMDHealth.WebClient
{
    internal abstract class RouteConstraint
    {
        private static readonly IDictionary<string, RouteConstraint> _cachedConstraints
            = new Dictionary<string, RouteConstraint>();

        public abstract bool Match(string pathSegment, out object convertedValue);

        public static RouteConstraint Parse(string template, string segment, string constraint)
        {
            if (string.IsNullOrEmpty(constraint))
            {
                throw new ArgumentException($"Malformed {nameof(segment)} '{segment}' in route '{template}' contains an empty {nameof(constraint)}.");
            }

            if (_cachedConstraints.TryGetValue(constraint, out RouteConstraint cachedInstance))
            {
                return cachedInstance;
            }
            else
            {
                RouteConstraint newInstance = CreateRouteConstraint(constraint);
                if (newInstance != null)
                {
                    _cachedConstraints[constraint] = newInstance;
                    return newInstance;
                }
                else
                {
                    throw new ArgumentException($"Unsupported {nameof(constraint)} '{constraint}' in route '{template}'.");
                }
            }
        }

        private static RouteConstraint CreateRouteConstraint(string constraint)
        {
            switch (constraint)
            {
                case "bool":
                    return new TypeRouteConstraint<bool>(bool.TryParse);
                case "datetime":
                    return new TypeRouteConstraint<DateTime>((string str, out DateTime result)
                        => DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out result));
                case "decimal":
                    return new TypeRouteConstraint<decimal>((string str, out decimal result)
                        => decimal.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out result));
                case "double":
                    return new TypeRouteConstraint<double>((string str, out double result)
                        => double.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out result));
                case "float":
                    return new TypeRouteConstraint<float>((string str, out float result)
                        => float.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out result));
                case "guid":
                    return new TypeRouteConstraint<Guid>(Guid.TryParse);
                case "int":
                    return new TypeRouteConstraint<int>((string str, out int result)
                        => int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out result));
                case "long":
                    return new TypeRouteConstraint<long>((string str, out long result)
                        => long.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out result));
                case "short":
                    return new TypeRouteConstraint<short>((string str, out short result)
                        => short.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out result));
                default:
                    return null;
            }
        }
    }
}
