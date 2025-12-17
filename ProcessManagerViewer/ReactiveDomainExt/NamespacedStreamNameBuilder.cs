using System;

using ReactiveDomain.Foundation;

namespace ReactiveDomain;

public class NamespacedStreamNameBuilder : IStreamNameBuilder {
	public string GenerateForAggregate(Type type, Guid id) {
		if (!type.Namespace?.Contains(".") ?? false) { throw new InvalidOperationException("Namespace cannot be root.  An example would be [Product].[Aggregate] ... A valid example would be [Product].[SecondLevel].[Aggregate]"); }
		var prefix = type.Namespace!.Substring(type.Namespace!.LastIndexOf(".") + 1);
		string value = (string.IsNullOrWhiteSpace(prefix) ? string.Empty : (prefix.ToLowerInvariant() + "."));
		return $"{value}{ToCamelCaseInvariant(type.Name)}-{id:N}";
	}

	public string GenerateForCategory(Type type) {
		if (!type.Namespace?.Contains(".") ?? false) { throw new InvalidOperationException("Namespace cannot be root.  An example would be [Product].[Aggregate] ... A valid example would be [Product].[SecondLevel].[Aggregate]"); }
		var prefix = type.Namespace!.Substring(type.Namespace!.LastIndexOf(".") + 1);
		return $"$ce-{prefix.ToLowerInvariant()}.{ToCamelCaseInvariant(type.Name)}";
	}

	public string GenerateForEventType(string type) {
		return $"$et-{type}";
	}

	private string ToCamelCaseInvariant(string name) {
		if (string.IsNullOrEmpty(name)) {
			return name;
		}

		if (1 == name.Length) {
			return name.ToLowerInvariant();
		}

		return char.ToLowerInvariant(name[0]) + name.Substring(1);
	}
}
