using UnityEngine;

public class EnableDisableFootIK : MonoBehaviour
{
	public SimpleHumanoidFootIK footIk;

	public void EnableDisable()
	{
		if (!(footIk == null))
		{
			footIk.ikEnabled = !footIk.ikEnabled;
		}
	}
}
