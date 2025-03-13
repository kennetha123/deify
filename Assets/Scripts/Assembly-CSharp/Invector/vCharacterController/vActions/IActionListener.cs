namespace Invector.vCharacterController.vActions
{
	public interface IActionListener : IActionEnterListener, IActionController, IActionExitListener, IActionStayListener
	{
		bool useActionEnter { get; }

		bool useActionExit { get; }

		bool useActionStay { get; }

		bool doingAction { get; }
	}
}
