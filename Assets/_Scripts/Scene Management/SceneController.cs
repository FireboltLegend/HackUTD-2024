using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MILab
{
    public class SceneController : MonoBehaviour
    {
        public static SceneController Instance { get; private set; }
        public static Action<string> OnSceneLoad = delegate { };

        [SerializeField] private List<DigitalTwinScene> _scenes;
        [SerializeField] private Material _defaultSkybox;
        [SerializeField, ReadOnly, Tooltip("First in list below that is enabled")] private DigitalTwinScene _sceneOnLoad;

        public List<DigitalTwinScene> Scenes => _scenes;

        private PhotonView photonView;
        private Dictionary<int, string> clientScenes; // Each actor number is associated with the scene that the client has open
        private Dictionary<int, int> clientAvatars; // Each photon viewID of an avatar is associated with its owning actor number

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this);
            clientScenes = new Dictionary<int, string>();
            clientAvatars = new Dictionary<int, int>();
            photonView = GetComponent<PhotonView>();
        }

        private void OnValidate()
        {
            CheckFirstSceneToLoad();
        }

        private void CheckFirstSceneToLoad()
        {
            foreach (var scene in _scenes.Where(scene => scene.Valid))
            {
                _sceneOnLoad = scene;
                return;
            }
        }

        private static int LocalActorNumber => PhotonNetwork.LocalPlayer.ActorNumber;
        public string CurrentScene
        {
            get
            {
                if (clientScenes.ContainsKey(LocalActorNumber))
                    return clientScenes[LocalActorNumber];
                Debug.LogError("Local Actor not contained in Client Scenes Dictionary");
                return "";
            }
        }

        public void LoadInitialScene()
        {
            CheckFirstSceneToLoad(); 
            if (_sceneOnLoad.Skybox == null) RenderSettings.skybox = _defaultSkybox;
            else RenderSettings.skybox = _sceneOnLoad.Skybox;

            photonView.RPC(nameof(RPC_LoadScene), RpcTarget.AllBuffered, LocalActorNumber, _sceneOnLoad.SceneName);
        }

        [Button(Mode = ButtonMode.NotInPlayMode)]
        public void LoadScene(int index) => LoadScene(_scenes[index].SceneName, _scenes[index].Skybox);
        private void LoadScene(string scene, Material skybox = null)
        {
            if (CurrentScene.Equals(scene)) return;
            
            // Change skybox to match next scene
            if (skybox == null) skybox = _defaultSkybox;
            RenderSettings.skybox = skybox;
            
            UnloadCurrent();
            photonView.RPC(nameof(RPC_LoadScene), RpcTarget.AllBufferedViaServer, LocalActorNumber, scene);
        }

        private void UnloadCurrent()
        {
            photonView.RPC(nameof(RPC_UnloadScene), RpcTarget.AllBufferedViaServer, LocalActorNumber, CurrentScene);
        }

        #region Loading
        
        // This RPC is called by a client adding a virtual scene
        [PunRPC]
        public void RPC_LoadScene(int actorNum, string sceneName)
        {
            if (!clientScenes.ContainsKey(actorNum)) clientScenes.Add(actorNum, "");
            if (!clientScenes[actorNum].Contains(sceneName)) clientScenes[actorNum] = sceneName;

            if (actorNum == LocalActorNumber)
            {
                // This is the local user joining the scene. Load the scene
                AsyncSceneLoad(sceneName);

                // For every other user in the scene, set this user to be visible if they are in the same scene
                foreach (int client in clientScenes.Keys)
                {
                    if (client == actorNum) continue;

                    bool sharedScene = clientScenes[client].Equals(sceneName);

                    var avatar = PhotonView.Find(clientAvatars[client]).GetComponent<PhotonAvatarEntity>();
                    avatar.SetVisibility(sharedScene);
                }
            }
            // If a remote user joins the same scene as the local user, set them to be visible
            else if (clientScenes[LocalActorNumber].Equals(sceneName))
            {
                var avatar = PhotonView.Find(clientAvatars[actorNum]).GetComponent<PhotonAvatarEntity>();
                avatar.SetVisibility(true);
            }
        }

        private void AsyncSceneLoad(string scene) => StartCoroutine(AsyncSceneLoadCoroutine(scene));
        private static IEnumerator AsyncSceneLoadCoroutine(string scene)
        {
            var async = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
            while (async.progress < 0.9f)
            {
                yield return null;
            }
            OnSceneLoad?.Invoke(scene);
        }
        
        #endregion

        #region Unloading
        
        // RPC called by a client leaving a virtual scene
        [PunRPC]
        public void RPC_UnloadScene(int actorNum, string sceneName)
        {
            if (!clientScenes.ContainsKey(actorNum)) clientScenes.Add(actorNum, "");
            if (clientScenes[actorNum].Equals(sceneName)) clientScenes[actorNum] = "";

            if (actorNum == LocalActorNumber)
            {
                // This is the local user leaving the scene. Unload the scene
                AsyncSceneUnload(sceneName);
                
                // For every other user in the scene, set this user to be visible if they are in the same scene
                foreach (int client in clientScenes.Keys)
                {
                    if (client == actorNum) continue;

                    bool sharedScene = clientScenes[client].Equals(sceneName);
                    var avatar = PhotonView.Find(clientAvatars[client]).GetComponent<PhotonAvatarEntity>();
                    avatar.SetVisibility(sharedScene);
                }
            }
            // If a remote user leaves the scene the local user is in, set them to no longer be visible
            else if (clientScenes[LocalActorNumber].Equals(sceneName))
            {
                var avatar = PhotonView.Find(clientAvatars[actorNum]).GetComponent<PhotonAvatarEntity>();
                avatar.SetVisibility(false);
            }

        }

        private void AsyncSceneUnload(string scene) => StartCoroutine(AsyncSceneUnloadCoroutine(scene));
        protected virtual IEnumerator AsyncSceneUnloadCoroutine(string scene)
        {
            var async = SceneManager.UnloadSceneAsync(scene);
            while (async.progress < 0.9f)
            {
                yield return null;
            }
        }
        
        #endregion

        // FIXME: Remove the avatar id from list if they disconnect
        [PunRPC]
        public void RPC_AddAvatar(int actorNum, int id)
        {
            clientAvatars.Add(actorNum, id);
        }

    }
}
