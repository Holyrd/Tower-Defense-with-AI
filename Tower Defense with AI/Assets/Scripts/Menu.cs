using UnityEngine;
using TMPro;

public class Menu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TextMeshProUGUI currencyUI;
	[SerializeField] Animator animator;

	private bool isMenuOpen = true;

	public void MenuToggle()
	{
		isMenuOpen = !isMenuOpen;
		animator.SetBool("Menu Open", isMenuOpen);
	}

	private void OnGUI()
	{
		currencyUI.text = LevelManager.main.currency.ToString();
	}

}
