using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using Vuforia;

/*
    This is the main class where GBRU module is tracked with Augmented reality using Vuforia,
    show diagrams, detect specific parts over it, shows circuits and more.
 */
public class PointerController : MonoBehaviour, ITrackableEventHandler
{

    public GameObject
        canvasRas,
        canvasConnection,
        canvasElectricalDiagram,
        electricalBtn,
        focusMessageText,
        focusMessage,
        fullDiagram,
        mainInputField,
        mapa_sensores,
        mapa_partes_generales,
        rasBtn,
        sensors_detected_string;

    private static List<GameObject> sensors_detected;
    public Electrical electricalTable;
    private string messageFocusString, messageObjectNotFound;
    public Toggle toggleGeneralPartsBtn, toggleFullBtn, toggleSensorsBtn, toggleRasBtn, toggleCircuitBtn;
    private GameObject[] gameObjectsWithObjectNameTag;
    public Text electricalConnectionString;
    private static bool rasFounded, circuitFounded;

    // DECLARE OBJECTS TO CLONE AND SHOW COMPONENT NAME AND POSITION 
    public GameObject TextObjectLayerName;
    public static GameObject CanvasForTextObjectLayer;

    IEnumerable<TrackableBehaviour> tbs;
    string arrSensors = "";

    TrackableBehaviour mTrackableBehaviour;

    public GameObject mapa_s { get; private set; }
    public GameObject panel { get; private set; }

    //SET AN INSTANCE FROM ELECTRICAL.CS
    // GET AN INSTANCE OF GAME OBJECTS FROM THE EDITOR IN THE AWAKE LIFE CYCLE.
    void Awake()
    {


        electricalTable = new Electrical();
        electricalTable.create();
        electricalConnectionString = GameObject.Find("TextElectricalConn").GetComponent<Text>();

        canvasRas = GameObject.Find("RAS");
        canvasConnection = GameObject.Find("ElectricalPanelConn");
        canvasElectricalDiagram = GameObject.Find("ConnectionPCB");
        CanvasForTextObjectLayer = GameObject.Find("CanvasForTextObjectLayer");
        panel = GameObject.Find("Panel");
        panel.transform.localScale = new Vector3(1, 1, 1);

        mapa_sensores = GameObject.Find("mapa_sensores");

        mapa_partes_generales = GameObject.Find("GENERALPARTS");

        fullDiagram = GameObject.Find("fullDiagram");

        circuitFounded = false;

        rasFounded = false;
        rasBtn = GameObject.Find("ToggleRasBtn");

        electricalBtn = GameObject.Find("ToggleElectricalBtn");
        sensors_detected_string = GameObject.Find("sensors_detected_string");
        sensors_detected = new List<GameObject>();

        TextObjectLayerName = GameObject.Find("TextObjectLayerName");
        focusMessage = GameObject.Find("PanelMessageFocus");
        setPanelFocus(true);

        //INSTANCE OF POPUP GET FOCUS
        focusMessageText = GameObject.Find("FocusMessage");
        messageFocusString = "FOCUS ON THE SIDE OF THE GBRU";
        messageObjectNotFound = "OBJECT NOT FOUND";
        focusMessageText.GetComponent<Text>().text = messageFocusString;

        //TOGGLE panel btns
        toggleGeneralPartsBtn = GameObject.Find("ToggleGeneralParts").GetComponent<Toggle>();
        toggleSensorsBtn = GameObject.Find("TogglesensorsBtn").GetComponent<Toggle>();
        toggleFullBtn = GameObject.Find("ToggleFullDiagramBtn").GetComponent<Toggle>();
        toggleCircuitBtn = GameObject.Find("ToggleElectricalBtn").GetComponent<Toggle>();
        toggleRasBtn = GameObject.Find("ToggleRasBtn").GetComponent<Toggle>();

    }


    /*
     * GET TrackableBehaviour:
     The base class for all TrackableBehaviours in Vuforia This class serves both as an 
     augmentation definition for a Trackable in the editor as well as a tracked Trackable result at runtime 

     REGISTER EVENT HANDLER FOR TRACKING IMAGE
     */
    void Start()
    {

        mTrackableBehaviour = GetComponent<TrackableBehaviour>();
        if (mTrackableBehaviour)
            mTrackableBehaviour.RegisterTrackableEventHandler(this);

    }


    /*
     * find every component in FRU Check and replace and call find part for each one
     - This function get text values from error founded in the search error section
     - Analize and split every string and separates words that can be founded as a part of the module eg: UGAS, EESM.. 
     - Finally tries to find parts calling FindPart() function
     */
    public void findCheckReplace()
    {
        //get text values from error founded in the search error section
        string componentsToSearchList = "";
        componentsToSearchList += GameObject.Find("relatedFruLabelMainValue").GetComponent<Text>().text;
        componentsToSearchList += " " + GameObject.Find("relatedFruLabelOtherValue").GetComponent<Text>().text;
        componentsToSearchList += " " + GameObject.Find("connectorLabelValue").GetComponent<Text>().text;
        componentsToSearchList += " " + GameObject.Find("sensorLabelValue").GetComponent<Text>().text;
        componentsToSearchList += " " + GameObject.Find("motorSolenoidLabelValue").GetComponent<Text>().text;


        // Analize and split every string and separates words that can be founded as a part of the module eg: UGAS, EESM.. 


        //create rule for split strings
        string[] stringSeparators = new string[] { " OR , ", " ", "/" };

        //SPLIT strings, take out words that can be between parts like OR, spaces.
        string[] errorCodesArray = componentsToSearchList.Split(stringSeparators, StringSplitOptions.None);

        //for each string in errorcodesarray call the FindPart(Text) 
        foreach (string val in errorCodesArray)
        {
            if (val != "")
            {
                GameObject textTempObj = new GameObject("TempText");
                textTempObj.AddComponent<Text>();
                textTempObj.GetComponent<Text>().text = val;

                FindPart(textTempObj.GetComponent<Text>());
                Destroy(textTempObj);
            }
        }

    }

    //ERASE ALL TEXT ELEMENTS DISPLAYED ON MODULE
    private void eraseGameObjectsWithObjectNameTag()
    {
        gameObjectsWithObjectNameTag = GameObject.FindGameObjectsWithTag("objectNameTag");

        foreach (GameObject obj in gameObjectsWithObjectNameTag) Destroy(obj);

    }


    // HIDE CANVAS RAS AND CHECK BUTTON RAS
    private void switchOffRas()
    {
        canvasRas.SetActive(false);
        rasBtn.SetActive(false);
    }

    // SHOW CANVAS RAS AND CHECK BUTTON RAS
    private void switchOnRas()
    {
        canvasRas.SetActive(true);
        rasBtn.SetActive(true);
    }


    // GIVEN CERTAIN TAG LIKE 51, IT SEARCHES FOR TEXTURES
    // IF TEXTURE OF RAS EXISTS, IT SHOWS TEXTURE RAS ON SCREEN
    private void searchRas(GameObject objToSearch)
    {
        try
        {
            // TRIES TO LOAD TEMP RAS TEXTURE
            Texture tex = Resources.Load("Textures/" + objToSearch.tag, typeof(Texture)) as Texture;

            // if RAS exists SET TEXTURE AND SHOW IT ON SCREEN
            if (tex)
            {
                rasFounded = true;
                canvasRas.GetComponent<Renderer>().material.SetTexture("_MainTex", tex);
                switchOnRas();
            }
            else
            {
                rasFounded = false;
                switchOffRas();
            }
        }
        catch (NullReferenceException ex)
        {
            Debug.Log(ex.Data);
            switchOffRas();
        }
    }


    /*
     SEARCH ELECTRICAL CONNECTION FOR CERTAIN COMPONENT AND SHOWS IT ON SCREEN
     */
    private void searchElectricalConn(GameObject objToSearch)
    {
        //CALL FUNCTION FROM ELECTRICAL.CS TO SEARCH SOME DEVICE CONNECTION
        string[] filaEncontrada = electricalTable.getDeviceElectricalDiagram(objToSearch.name);

        if (filaEncontrada != null)
        {
            try
            {
                // TRY TO FOUND PCB IMAGE - TEXTURE
                Texture _pcbTex = Resources.Load("Textures/" + filaEncontrada[3], typeof(Texture)) as Texture;

                if (_pcbTex)
                {
                    canvasElectricalDiagram.GetComponent<Renderer>().material.SetTexture("_MainTex", _pcbTex);
                    electricalConnectionString.text = string.Format("   " + filaEncontrada[0] + " goes to cable connector{0}↓{0}" + filaEncontrada[1] + "  to PCB : {0}↓{0}" + filaEncontrada[2] + "(" + filaEncontrada[3] + ")", Environment.NewLine);
                    circuitFounded = true;
                    electricalBtn.SetActive(true);
                }
                else
                {
                    circuitFounded = false;
                }
            }
            catch (NullReferenceException ex)
            {
                Debug.Log(ex.Data);
                canvasConnection.SetActive(false);
                electricalBtn.SetActive(false);
            }

        }


    }

    //SET OBJECT NAME AND VISUALIZE ON MODULE 
    //MAKE A COPY FOR ANY OBJECT FOUNDED  
    private void setNameObject(GameObject objToSearch)
    {
        if (objToSearch.name == "SENSORS") return;
        GameObject canvasClone = Instantiate(CanvasForTextObjectLayer, objToSearch.transform.localPosition, Quaternion.identity, CanvasForTextObjectLayer.transform.parent);
        //Instantiate(TextObjectLayerName, sensorFounded.transform.localPosition, Quaternion.identity);
        canvasClone.transform.GetChild(0).gameObject.GetComponent<Text>().text = objToSearch.name;
        canvasClone.transform.localPosition = objToSearch.transform.localPosition;
        canvasClone.name = "textObjectFounded_" + objToSearch.name;
        canvasClone.tag = "objectNameTag";

    }


    // TRIES TO FIND ANY NAME OBJECT THAT MATCHES WITH findThisObject AND MAKE ACTIVE ON SCREEN
    public void FindPart(Text findThisObject)
    {

        string objectToSearch;
        objectToSearch = findThisObject.text;
        bool _esSensor, _esSolenoide, _esMotor;
        tbs = TrackerManager.Instance.GetStateManager().GetActiveTrackableBehaviours();

        //Check if gbru is tracked

        if (tbs.Count() > 0)
        {
            //capture tracked gbru gameobject
            GameObject targetedObj = tbs.First().gameObject;

            //turn chars to upper
            objectToSearch = objectToSearch.ToUpper();

            //check object type: sensor or solenoid or motor
            _esSensor = targetedObj.transform.Find("SENSORS").Find(objectToSearch);
            _esSolenoide = targetedObj.transform.Find("SOLENOIDS").Find(objectToSearch);
            _esMotor = targetedObj.transform.Find("MOTORS").Find(objectToSearch);

            if (_esSensor || _esSolenoide || _esMotor)
            {
                GameObject sensorFounded;

                if (_esSensor)
                {
                    sensorFounded = targetedObj.transform.Find("SENSORS").Find(objectToSearch).gameObject;
                }
                else if (_esMotor)
                {
                    sensorFounded = targetedObj.transform.Find("MOTORS").Find(objectToSearch).gameObject;
                }
                else
                {
                    sensorFounded = targetedObj.transform.Find("SOLENOIDS").Find(objectToSearch).gameObject;
                }

                /*
                   - SENSORS DETECTED IS A GAME OBJECT LIST
                   - arrSensors es un string que contiene los nombres de todos los objetos encontrados 
                   - BUSCA SI SENSOR YA EXISTE EN LISTA DE SENSORES ENCONTRADOS
                   - SI NO EXISTE EN LISTA, SE AGREGA A LA MISMA
                   - SI LA LISTA NO TIENE OBJECTOS SE AGREGA EL OBJ ENCONTRADO
                 */
                if (sensors_detected.Count == 0 || !sensorInList(sensorFounded.name))
                {
                    sensors_detected.Add(sensorFounded);
                    arrSensors += sensorFounded.name + ", ";

                    //VISUALIZA SENSOR ENCONTRADO
                    sensorFounded.SetActive(true);

                    //SET OBJECT NAME AND VISUALIZE ON MODULE 
                    //MAKE A COPY FOR ANY OBJECT FOUNDED  
                    setNameObject(sensorFounded);
                }

                //CALL SEARCH RAS TO SEARCH FOR RAS TEXTURE
                searchRas(sensorFounded);

                //CALL TO SEARCH IF PART HAS AN ELECTRICAL CONNECTION TO DISPLAY
                searchElectricalConn(sensorFounded);

                // STRING THAT CONCATENATES EVERY PART FOUNDED, TO SHOW IN SCREEN
                sensors_detected_string.GetComponent<Text>().text = "Showing: " + arrSensors;

                toggleRas();
                toggleCircuit();
            }
        }
    }

    //SET INACTIVE EVERY PART ACTIVE ON MODULE
    public void deleteActiveSensors()
    {
        eraseGameObjectsWithObjectNameTag();

        sensors_detected.ForEach(delegate (GameObject sensor)
        {

            sensor.SetActive(false);
        });
        arrSensors = "";
        sensors_detected.Clear();
        sensors_detected_string.GetComponent<Text>().text = "";
    }

    public void closeApp()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        Application.Quit();
    }

    public void ToggleGeneralParts()
    {

        if (toggleGeneralPartsBtn.isOn)
        {
            //resetPanel();
            //toggleGeneralPartsBtn.isOn = true;
            mapa_partes_generales.SetActive(true);
            toggleSensorsBtn.isOn = false;
            toggleFullBtn.isOn = false;


            mapa_sensores.SetActive(false);
            canvasRas.SetActive(false);
            fullDiagram.SetActive(false);

        }
        else
        {
            mapa_partes_generales.SetActive(false);

        }


    }

    public void ToggleMapSensors()
    {

        if (toggleSensorsBtn.isOn)
        {
            toggleFullBtn.isOn = false;
            toggleGeneralPartsBtn.isOn = false;
            fullDiagram.SetActive(false);
            mapa_sensores.SetActive(true);
            mapa_partes_generales.SetActive(false);

        }
        else
        {
            mapa_sensores.SetActive(false);

        }


    }

    public void toggleFull()
    {

        if (toggleFullBtn.isOn)
        {
            toggleSensorsBtn.isOn = false;
            toggleGeneralPartsBtn.isOn = false;
            fullDiagram.SetActive(true);
            mapa_sensores.SetActive(false);
            mapa_partes_generales.SetActive(false);
        }
        else
        {
            fullDiagram.SetActive(false);

        }
    }

    public void switchOffSensorsAndFull()
    {
        toggleSensorsBtn.isOn = false;
        toggleFullBtn.isOn = false;
        mapa_sensores.SetActive(false);
        fullDiagram.SetActive(false);
        mapa_partes_generales.SetActive(false);
    }

    public void toggleRas()
    {
        if (toggleRasBtn.isOn && rasFounded)
        {
            toggleCircuitBtn.isOn = false;
            canvasConnection.SetActive(false);
            canvasRas.SetActive(true);
        }
        else
        {
            canvasRas.SetActive(false);
        }

        fullDiagram.SetActive(false);
        mapa_sensores.SetActive(false);
        mapa_partes_generales.SetActive(false);
    }

    public void toggleCircuit()
    {
        if (toggleCircuitBtn.isOn && circuitFounded)
        {
            toggleRasBtn.isOn = false;
            canvasRas.SetActive(false);
            canvasConnection.SetActive(true);

        }
        else
        {
            canvasConnection.SetActive(false);
        }

        fullDiagram.SetActive(false);
        mapa_sensores.SetActive(false);
        mapa_partes_generales.SetActive(false);
    }

    //CHECK IF PART FOUNDED WAS ALREADY IN LIST
    bool sensorInList(string sensorFounded)
    {
        bool result = false;
        foreach (GameObject sensorInList in sensors_detected)
        {

            if (sensorInList.name == sensorFounded)
            {
                result = true;
            }


        }
        return result;
    }


    //SHOWS POP UP IF MODULE WAS TRACKED OR NOT
    public void setPanelFocus(bool val)
    {
        focusMessage.SetActive(val);
    }

    //GET IF TRACKABLE WAS CATCHED OR NOT
    // IF TRACKED SHOWS AR DIAGRAMS ON IT
    public void OnTrackableStateChanged(TrackableBehaviour.Status previousStatus, TrackableBehaviour.Status newStatus)
    {

        String status = newStatus.ToString();
        //if not tracked
        if ((status == "NOT_FOUND" || status == "UNKNOWN") && panel)
        {
            setPanelFocus(true);

        }
        else
        {
            //if tracked

            setPanelFocus(false);
            
            toggleFullBtn.isOn = false;
            toggleRasBtn.isOn = false;
            toggleCircuitBtn.isOn = false;
            rasBtn.SetActive(false);
            electricalBtn.SetActive(false);
            fullDiagram.SetActive(false);
            mapa_sensores.SetActive(false);
            toggleGeneralPartsBtn.isOn = true;
            mapa_partes_generales.SetActive(true);

        }
        canvasRas.SetActive(false);
        canvasConnection.SetActive(false);
    }
}


