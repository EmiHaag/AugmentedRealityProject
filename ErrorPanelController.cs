using System;
using UnityEngine;
using UnityEngine.UI;

using System.Data;

/*
  PANEL IN WHICH WE DISPLAY DETAILS OF ERROR FOUNDED 
 */
public class ErrorPanelController : MonoBehaviour
{

    public Toggle toggleErr, toggleCheck, toggleProbable;
    public GameObject
        causesError,
        causesFirstColValue,
        causesSecAndThirdColsValue,
        checkReplace,
        command_code,
        connectorLabelValue,
        disPlayErrorPanel,
        detailMStatus,
        detail_command_code,
        errorMessage,
        error_code,
        errorCodeMessageValue,
        errorCodeDetectValue,
        hidePanelBtn,
        m_data,
        m_status,
        motorSolenoidLabelValue,
        relatedFru,
        relatedFruLabelMainValue,
        relatedFruLabelOtherValue,
        sensorLabelValue;

    public Text TextBtnHidePanel;
    private string m_statusText, m_dataText, command_code_description;
    static DataRow DataRowError;
    static DataTable DataTableCausesList;
    private ErrorCodes errorCodes;
    static int currentCauseErrorIndex;
    static string currentCausesCodesString;


    void Awake()
    {
        causesError = GameObject.Find("causesError").gameObject;
        causesFirstColValue = GameObject.Find("causesFirstColValue").gameObject;
        causesSecAndThirdColsValue = GameObject.Find("causesSecAndThirdColsValue").gameObject;
        checkReplace = GameObject.Find("checkReplace").gameObject;
        errorMessage = GameObject.Find("errorMessage").gameObject;
        connectorLabelValue = GameObject.Find("connectorLabelValue").gameObject;
        hidePanelBtn = GameObject.Find("hidePanelBtn").gameObject;
        motorSolenoidLabelValue = GameObject.Find("motorSolenoidLabelValue").gameObject;
        m_status = GameObject.Find("TextMStatus").gameObject;
        m_data = GameObject.Find("TextMData").gameObject;
        m_statusText = "no data";
        m_dataText = "no data";
        relatedFruLabelMainValue = GameObject.Find("relatedFruLabelMainValue").gameObject;
        relatedFruLabelOtherValue = GameObject.Find("relatedFruLabelOtherValue").gameObject;
        sensorLabelValue = GameObject.Find("sensorLabelValue").gameObject;
        detailMStatus = GameObject.Find("mStatusLabelValue").gameObject;
        detail_command_code = GameObject.Find("commandCodeLabelValue").gameObject;
        errorCodeMessageValue = GameObject.Find("errorCodeMessageValue").gameObject;
        errorCodeDetectValue = GameObject.Find("errorCodeDetectValue").gameObject;
        disPlayErrorPanel = GameObject.Find("DETAILERRORSPANEL").gameObject;

        DataTableCausesList = new DataTable();

        TextBtnHidePanel = GameObject.Find("TextBtnHidePanel").GetComponent<Text>();
        toggleErr = GameObject.Find("ToggleErrorMessage").GetComponent<Toggle>();
        toggleCheck = GameObject.Find("ToggleCheckReplace").GetComponent<Toggle>();
        toggleProbable = GameObject.Find("ToggleProbableCauses").GetComponent<Toggle>();

    }

    void Start()
    {

        //disable show/hide button from panel
        hidePanelBtn.GetComponent<Button>().enabled = false;

        //hide panel error details
        hidePanel();

        //init error table
        errorCodes = new ErrorCodes();
        errorCodes.create();


    }

    //toggle to show or hide detail error panel
    public void hidePanel()
    {
        float y_show, y_hide;
        y_show = 251;
        y_hide = 0;
        if (disPlayErrorPanel.transform.localScale.y == y_show)
        {
            disPlayErrorPanel.transform.localScale = new Vector3(disPlayErrorPanel.transform.localScale.x, y_hide, 1);
            TextBtnHidePanel.text = "Show details";
        }
        else
        {
            disPlayErrorPanel.transform.localScale = new Vector3(disPlayErrorPanel.transform.localScale.x, y_show, 1);
            TextBtnHidePanel.text = "Hide details";
        }
    }

    //show detail error panel when click on Go! searching error button
    public void showPanel()
    {
        float y_show;
        y_show = 251;
        //check if show/hide button is disabled, if false set to true
        if (!hidePanelBtn.GetComponent<Button>().enabled)
            hidePanelBtn.GetComponent<Button>().enabled = true;

        disPlayErrorPanel.transform.localScale = new Vector3(disPlayErrorPanel.transform.localScale.x, y_show, 1);
        TextBtnHidePanel.text = "Hide details";
    }
    //ESTA CLASE UTILIZA ERRORCODES.CS COMO INTERFACE PARA OBTENER LA INFO NECESARIA DE MS MDATA


    public void setNextCurrentCauseListRow()
    {

        if (currentCauseErrorIndex < (DataTableCausesList.Rows.Count - 1))
            currentCauseErrorIndex++;
        else currentCauseErrorIndex = 0;


        if (currentCausesCodesString != "" && DataTableCausesList.Rows.Count > 0 && currentCauseErrorIndex <= DataTableCausesList.Rows.Count)
        {


            //set current row
            DataRow row1 = DataTableCausesList.Rows[currentCauseErrorIndex];
            //set values descriptions
            causesFirstColValue.GetComponent<Text>().text = "[" + row1.ItemArray[0].ToString() + "] " + row1.ItemArray[1].ToString() + ": " + row1.ItemArray[2].ToString() + "\n\n";
            causesSecAndThirdColsValue.GetComponent<Text>().text =
                "Where to check: " + row1.ItemArray[3].ToString() + "\n\n" +
                "Measures to be taken: " + row1.ItemArray[4].ToString() + "\n   ";

        }




    }

    public void searchError()
    {
        //hidePanel();
        checkReplace.SetActive(false);
        errorMessage.SetActive(false);
        causesError.SetActive(false);
        toggleError();

        string m_status_description;
        m_dataText = m_data.GetComponent<Text>().text;
        m_statusText = m_status.GetComponent<Text>().text;

        m_status_description = errorCodes.getMStatusDescription(m_statusText);


        //split m-data into command code and error code to search in arrays
        string command_code_str, error_code_str;


        //check if m_dataText have the correct length
        try
        {
            command_code_str = m_dataText.Substring(0, 4).ToUpper();
            command_code_description = errorCodes.getCommandCodeStringFromMData(command_code_str);

        }
        catch (ArgumentOutOfRangeException e)
        {
            Debug.Log(e.Data);
            command_code_description = "Data missing";
        }

        //check if error code was typed
        if (m_dataText.Length == 8)
        {

            error_code_str = m_dataText.Substring(4, 4).ToUpper();
            DataRowError = errorCodes.getErrorCodeStringFromMData(error_code_str);

        }


        //RESULTS:

        detailMStatus.GetComponent<Text>().text = m_status_description;


        detail_command_code.GetComponent<Text>().text = command_code_description;

        if (DataRowError != null)
        {
            errorCodeMessageValue.GetComponent<Text>().text = (string)DataRowError[1];
            errorCodeDetectValue.GetComponent<Text>().text = (string)DataRowError[2];
            relatedFruLabelMainValue.GetComponent<Text>().text = (string)DataRowError[4];
            relatedFruLabelOtherValue.GetComponent<Text>().text = (string)DataRowError[5];

            //this 3 values are going to set new active labels on gbru 
            connectorLabelValue.GetComponent<Text>().text = (string)DataRowError[6];
            sensorLabelValue.GetComponent<Text>().text = (string)DataRowError[7];
            motorSolenoidLabelValue.GetComponent<Text>().text = (string)DataRowError[8];


            if ((string)DataRowError[9].ToString() != "")
            {
                currentCausesCodesString = (string)DataRowError[9];
                DataTableCausesList = errorCodes.getCausesValueFromErrorCodeStr(currentCausesCodesString);
                currentCauseErrorIndex = -1;
                setNextCurrentCauseListRow();

            }
            errorMessage.SetActive(true);

        }
    }


    public void toggleError()
    {
        if (toggleErr.isOn)
        {
            checkReplace.SetActive(false);
            causesError.SetActive(false);
            errorMessage.SetActive(true);

            toggleCheck.isOn = false;
            toggleProbable.isOn = false;
            toggleErr.isOn = true;
        }
    }
    public void toggleCheckReplace()
    {
        if (toggleCheck.isOn)
        {

            causesError.SetActive(false);
            checkReplace.SetActive(true);
            errorMessage.SetActive(false);

            toggleErr.isOn = false;
            toggleProbable.isOn = false;

            // relatedFruLabelMainValue.SetActive(true);
        }
    }
    public void toggleProbableCauses()
    {
        if (toggleProbable.isOn)
        {
            checkReplace.SetActive(false);
            errorMessage.SetActive(false);
            causesError.SetActive(true);

            toggleErr.isOn = false;
            toggleCheck.isOn = false;
        }
    }

}
