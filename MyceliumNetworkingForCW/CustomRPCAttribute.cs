using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyceliumNetworking
{
	[AttributeUsage(AttributeTargets.Method)]
	public class CustomRPCAttribute : Attribute
	{
		public CustomRPCAttribute()
		{

		}
	}
}
