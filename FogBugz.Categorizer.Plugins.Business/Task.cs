namespace FogBugz.Categorizer.Plugins.Business
{
	public class Task
	{
		public Project Project { get; set; }
		public string Name { get; set; }

		public bool Equals(Task other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.Project, Project) && Equals(other.Name, Name);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (Task)) return false;
			return Equals((Task) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Project != null ? Project.GetHashCode() : 0)*397) ^ (Name != null ? Name.GetHashCode() : 0);
			}
		}

		public override string ToString()
		{
			return string.Format("Project: {0} Task: {1}", Project.Name, Name);
		}
	}
}