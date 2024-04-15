using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class BuilderSpawner : MonoBehaviour {

    public static BuilderSpawner instance;
    public DungeonGeneratorCTR generator;
    [SerializeField] string designFileName;
    // Start is called before the first frame update
    void Awake() {
         if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);


        XmlSerializer serializer = new XmlSerializer(typeof(Setup));
        FileStream file = new FileStream(designFileName, FileMode.Open);
        Setup setup = (Setup)serializer.Deserialize(file);

        generator = this.GetComponentInChildren<DungeonGeneratorCTR>();
        generator.InitFromSetup(setup, setup.map.Seed);
    }

    // Update is called once per frame
    void Update() {
        bool mouseWasPressed = Input.GetMouseButtonDown(0);

        if (mouseWasPressed) {
            Vector2Int forward = new Vector2Int((int)transform.forward.x, (int)transform.forward.z);
            Vector2Int location = new Vector2Int((int)transform.position.x, (int)transform.position.z);
            Debug.Log(location);
            generator.CreateRoomie(location+forward, forward, 0, 9, 0, 12, DungeonResources.RoomSize.MEDIUM, 0);
            generator.OverrideIteration();
        }

        generator.SetEdges();
        generator.SetCeiling();
        generator.SetLights();
        generator.PlaceTunnels();
        generator.PlaceAnteRooms();
        generator.PlaceRooms();
    }
}
