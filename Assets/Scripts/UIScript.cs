using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIScript : MonoBehaviour{

    private static UIScript instance;
    public static UIScript Instance { get => instance;}
    [SerializeField]Image fuelImage;
    [SerializeField]TextMeshProUGUI magazineText;
    [HideInInspector] public float playerFuel;
    [HideInInspector] public float currentMagazine;

    private void Awake() {
        if(instance == null){
            instance = this;
        }
    }

    private void Update() {
        fuelImage.fillAmount = playerFuel / 100;
        if(currentMagazine > 0){
            magazineText.text = currentMagazine.ToString();
        }
        else{
            magazineText.text = "[R]eload";
        }
    }
}
