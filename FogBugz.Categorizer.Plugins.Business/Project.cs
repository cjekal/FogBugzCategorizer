namespace FogBugz.Categorizer.Plugins.Business
{
	public class Project
	{
		public string Name { get; set; }

		public bool Equals(Project other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.Name, Name);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (Project)) return false;
			return Equals((Project) obj);
		}

		public override int GetHashCode()
		{
			return (Name != null ? Name.GetHashCode() : 0);
		}

		public override string ToString()
		{
			return string.Format("Project: {0}", Name);
		}
	}
}