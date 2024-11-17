using System;
using System.IO;
using UnityEngine;

namespace MILab
{
    public class US1ControlAndData : MonoBehaviour
    {
        [SerializeField] Error_calculator errorCalc;
        [SerializeField] US1Manager us1Man;

        [Header("Study Params")]
        [SerializeField] int conditionNumber = 1;

        [SerializeField, ReadOnly] private float timer = 0f;
        private bool isRunning = false;
        private string path = "D:\\MetaTwin-US1"; // Replace with your desired file path

        float posError = 0;
        float rotError = 0;
        float posOffset = 0;
        float rotOffset = 0;
        int comboID = 0;
        int timeLag = 0;

        [Header("References")]
        [SerializeField] Questionnaire questionnaire;

        // Function to start the timer
        [Button]
        public void StartTimer()
        {
            timer = 0f;
            isRunning = true;
            HideQuestionnaire();
        }

        // Function to stop the timer and save to a file
        [Button]
        public void StopTimer()
        {
            isRunning = false;

            // Format timer to minutes and seconds
            TimeSpan timeSpan = TimeSpan.FromSeconds(timer);
            string formattedTime = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);

            // Calculate the error rate
            (posError, rotError) = errorCalc.GetError();

            // Get condition parameters
            (posOffset, rotOffset, comboID, timeLag) = us1Man.GetConditionParams();

            // Create file name with timestamp
            string fileName = "US1_Cond_"+ conditionNumber +"_Data_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
            string fullPath = Path.Combine(path, fileName);

            // Write the formatted time to the file
            try
            {
                string output = "Timer: " + formattedTime + "\n" +
                                "Position Error: " + posError + "\n" +
                                "Rotation Error: " + rotError + "\n\n" +
                                "-- Conditions Params --" + "\n" +
                                "Condition: " + conditionNumber + "\n" +
                                "Position Offset: " + posOffset + "\n" +
                                "Rotation Offset: " + rotOffset + "\n" +
                                "Time Lag: " + timeLag + "\n" +
                                "Combination: " + comboID;

                File.WriteAllText(fullPath, output);
                Debug.Log("Timer saved to " + fullPath);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to save timer: " + e.Message);
            }

            conditionNumber += 1;

            questionnaire.ToggleVisibility(true);
        }

        [Button]
        void ResetMoveableObjects()
        {
            // Reset the moveable objects to their positions
            us1Man.ResetMoveableObjects();
        }

        [Button]
        void HideQuestionnaire()
        {
            questionnaire.ToggleVisibility(false);
        }

        // Update is called once per frame
        private void Update()
        {
            if (isRunning)
            {
                timer += Time.deltaTime;
            }
        }
    }
}