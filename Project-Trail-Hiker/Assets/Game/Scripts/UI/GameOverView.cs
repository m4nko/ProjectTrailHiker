using System;
using Game.Scripts.Player;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Game.Scripts.UI
{
    public class GameOverView : Ui
    {
        private GameManager.GameManager gameManager;

        [SerializeField] private GameObject[] clocks = new GameObject[3];
        [SerializeField] private GameObject[] stars = new GameObject[3];
        [SerializeField] private TextMeshProUGUI newBottles;
        
        [FormerlySerializedAs("newTotalBottles")] 
        [SerializeField] private TextMeshProUGUI newTotalBottlesLabel;
        
        private int newTotalBottles;
        //private int oldTotalBottles;
        [SerializeField] private TextMeshProUGUI levelNumber;

        private PlayerInventory playerInventory;
        public void Start()
        {
            gameManager = FindObjectOfType<GameManager.GameManager>();
            playerInventory = FindObjectOfType<PlayerInventory>();
            
            //oldTotalBottles = playerInventory.Bottles;
            for (int i = 0; i < ScoreManager.ClockNumber; i++)
                clocks[i].gameObject.SetActive(true);

            for (int i = 0; i < ScoreManager.StarNumber; i++)
                stars[i].gameObject.SetActive(true);
            
            playerInventory.GainBottles((ScoreManager.ClockNumber + ScoreManager.StarNumber));

            //PlayerPrefs.SetInt("Bottle", oldTotalBottles + (ScoreManager.clockNumber + ScoreManager.starNumber));
            newTotalBottles = playerInventory.Bottles;
            
            SetUiTexts();
        }

        private void SetUiTexts()
        {
            newBottles.text = "+ " + (ScoreManager.ClockNumber + ScoreManager.StarNumber);
            
            newTotalBottlesLabel.text = newTotalBottles.ToString();

            levelNumber.text = "LEVEL " + Int32.Parse(SceneManager.GetActiveScene().name.Substring(5)) + " COMPLETO!";
        }

        public void TryAgainButtonPressed()
        {
            gameObject.SetActive(false);
            GameManager.GameStart();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        }

        public void QuitButtonPressed()
        {
            gameManager.DisableGameRunning();
            gameManager.DisablePause();
            Time.timeScale = 1.0f;
            SceneManager.LoadScene("InitialScene");
        }
    }
}
