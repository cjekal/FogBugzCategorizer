namespace FogBugz.Categorizer.Plugins.Business
{
	public class Template
	{
		public string Name { get; set; }

		public bool Equals(Template other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.Name, Name);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof(Template)) return false;
			return Equals((Template)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Name != null ? Name.GetHashCode() : 0);
			}
		}

		public override string ToString()
		{
			return string.Format("Template: {0} ", Name);
		}
	}
}