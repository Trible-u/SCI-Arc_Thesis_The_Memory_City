using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class Window : MonoBehaviour {

    private void Start() {
        transform.Find("attackTooltipBtn").GetComponent<Button_UI>().MouseOverOnceFunc = () => Tooltip.ShowTooltip_Static("Attack damage");
        transform.Find("attackTooltipBtn").GetComponent<Button_UI>().MouseOutOnceFunc = () => Tooltip.HideTooltip_Static();
        transform.Find("speedTooltipBtn").GetComponent<Button_UI>().MouseOverOnceFunc = () => Tooltip.ShowTooltip_Static("Movement speed");
        transform.Find("speedTooltipBtn").GetComponent<Button_UI>().MouseOutOnceFunc = () => Tooltip.HideTooltip_Static();
        transform.Find("healthTooltipBtn").GetComponent<Button_UI>().MouseOverOnceFunc = () => Tooltip.ShowTooltip_Static("Health amount");
        transform.Find("healthTooltipBtn").GetComponent<Button_UI>().MouseOutOnceFunc = () => Tooltip.HideTooltip_Static();
        
        transform.Find("attackBtn").GetComponent<Button_UI>().MouseOverOnceFunc = () => Tooltip.ShowTooltip_Static("Attack");
        transform.Find("attackBtn").GetComponent<Button_UI>().MouseOutOnceFunc = () => Tooltip.HideTooltip_Static();
        transform.Find("defendBtn").GetComponent<Button_UI>().MouseOverOnceFunc = () => Tooltip.ShowTooltip_Static("Defend");
        transform.Find("defendBtn").GetComponent<Button_UI>().MouseOutOnceFunc = () => Tooltip.HideTooltip_Static();
        transform.Find("patrolBtn").GetComponent<Button_UI>().MouseOverOnceFunc = () => Tooltip.ShowTooltip_Static("Patrol");
        transform.Find("patrolBtn").GetComponent<Button_UI>().MouseOutOnceFunc = () => Tooltip.HideTooltip_Static();
    }

}
