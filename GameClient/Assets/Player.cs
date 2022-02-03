using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {

    [SerializeField] private Slider slider;

    public Guid ID;

    private bool local;

    private Vector3 lastPosition;
    private Vector3 newPosition;

    private bool moveRandom = false;

    private float elapsed = 0;
    private bool right = false;

    public void Initialize() {
        local = true;
        ID = Guid.NewGuid();
        GetComponentInChildren<TextMeshProUGUI>().text = ID.ToString();
        GetComponentInChildren<TextMeshProUGUI>().color = Color.green;
        transform.Translate(new Vector3(UnityEngine.Random.Range(-5, 5), 0, 0));
    }

    public void UpdatePosition(Vector3 position) {
        lastPosition = transform.position;
        newPosition = position;
        //transform.position = position;
    }

    public void UpdateID(Guid id) {
        ID = id;
        GetComponentInChildren<TextMeshProUGUI>().text = ID.ToString();
    }

    private void Update() {
        if (local) {
            if (Input.GetKeyDown(KeyCode.Space)) {
                moveRandom = !moveRandom;
            }

            if (moveRandom) {
                elapsed += Time.deltaTime;
                if (elapsed > 1) {
                    elapsed = 0;
                    right = !right;
                }
                if (right) {
                    transform.Translate(Vector3.right * 0.01f);
                }
                else {
                    transform.Translate(Vector3.left * 0.01f);
                }
            }
            else {
                var horizontal = Input.GetAxis("Horizontal");
                var vertical = Input.GetAxis("Vertical");

                transform.Translate(new Vector3(horizontal, vertical, 0));
            }

        }
        else {
            transform.position = Vector3.Lerp(lastPosition, newPosition, slider.value);
        }
    }


    //public void SendData() {
    //    throw new System.NotImplementedException();
    //}
}
