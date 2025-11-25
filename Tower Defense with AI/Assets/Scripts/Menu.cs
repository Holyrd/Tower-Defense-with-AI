using UnityEngine;
using TMPro;

public class Menu : MonoBehaviour
{
    [Header("References")]
	[SerializeField] Animator animator;

	private bool isMenuOpen = true;

	public void MenuToggle()
	{
		isMenuOpen = !isMenuOpen;
		animator.SetBool("Menu Open", isMenuOpen);
	}
}
