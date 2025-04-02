using Fusion;
using UnityEngine;

namespace Gameplay
{
	public struct NetworkInputData : INetworkInput
	{
		public Vector2 Movement;
		public Vector2 Look;
		public NetworkBool Jump;
		public NetworkBool Sprint;
	}
}