using Eshava.Core.Linq.Enums;
using Eshava.Core.Linq.Models;

namespace Eshava.Test.Core.Linq.Models
{
	public class SortingModel
	{
		public SpecialSortField Beta { get; set; }
		public SortField Epsilon { get; set; }
		public SortOrder Gamma { get; set; }
		public SortField Rho { get; set; }
		public SortField Sigma { get; set; }

		public OmegaSortingModel Kappa { get; set; }

	}

	public class OmegaSortingModel : NestedSort
	{
		public SortField Psi { get; set; }
		public SortField Chi { get; set; }

	}
}