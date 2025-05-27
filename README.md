# NCGF
New Common Game Framework - tools for custom UI in C# / Unity

/*
 * [//]
 * 
 * ----------------------------------------
 * ||  NEW COMMON GAME FRAMEWORK (NCGF)  ||
 * ----------------------------------------
 * 
 * AUTHOR:      Louis Dufresne
 * VERSION:     0.1.c
 * 
 * 
 * DESCRIPTION:
 * 
 *      NCGF is a collection of modules designed to perform and organize various game mechanics. It is intended as a generic framework for
 *      the development of user interface, text display, object display, object manipulation, and other systems in 2D space. The framework
 *      is intended for custom use as an alternative to Unity's built-in systems.
 *      
 *      Modules included in NCGF include functionality for:
 *          // User input collection
 *          // User input actuation, prioritization, and events
 *          // Buttons, text fields, and other user input objects
 *          // Custom text rendering
 *          // Custom menu creation
 *          // Custom dialogue systems
 *          // Object sprite and mesh visuals
 *          // Object pooling
 *
 * INTEGRATION PROCEDURE:
 * 
 *      To integrate this framework into a project, perform the following steps:
 *      
 *          1)  Drag the NCGF Prefab (main folder) into the hierarchy.
 *      
 *          2)  Drag a reference to the current 2D camera into InputCollector ("Camera").
 *      
 *          3a) Attach NCGF_Cam_H_Camera to the camera.
 *          3b) Drag a reference to CamData into the camera ("Cd").
 *          3c) Drag a reference to the camera into itself ("This Camera").
 *          
 *          4)  If the current 2D camera is a UI camera for 3D, set camera to 'overlay', assign culling mask / layer, and add to 3D camera.
 *      
 * 
 * NAMING CONVENTIONS:
 * 
 *      Scripts will share a name with a class within that script. Some classes may share a script; in this case, the namesake class will be empty.
 * 
 *      All class names (excepting possibly object names) will begin with the prefix 'NCGF' followed by a '_' (underscore).
 *      
 *      Class names will then follow with a secondary prefix denoting its specific module, followed by a '_' (underscore).
 *      
 *      Class names will be categorized by their function using a letter code. This letter code will follow the aforementioned prefixes and will be
 *      followed by a '_' (underscore).
 *      
 *      The letter codes are as follows:
 *          // RG   -- Resource GameObjects. These are independently functioning MonoBehaviours, and must usually be instantiated ONCE.
 *          // D    -- Data Classes (static). These store publically accessible and alterable values and object references within a STATIC class.
 *          // R    -- Readonly Data Classes (static). These store unalterable (at runtime) parameters for program function within a STATIC class.
 *          // O    -- Objects, which are not GameObjects (do not derive from MonoBehaviour).
 *          // GO   -- GameObjects. Generally this denotes a MonoBehaviour that may be instanced many times across the program.
 *          // H    -- Handlers. These are MonoBehaviours generally in charge of a significant, automated operation or module.
 *          // S    -- Static Classes. These are classes whose operations are externally controlled.
 *      
 *      Lastly, classes will receive a unique name describing their function, after all aforementioned prefixes.
 *      
 * EXCEPTIONS:
 *      
 *      Naming conventions are flexible. If a module only contains one script, or if a script's namesake class is empty, that script and class may
 *      forego a secondary prefix and letter code.
 *      
 *      Likewise, objects which may be instantiated or referenced across the broader program (outside of NCGF-specific scripts) may only be prefixed
 *      by their functional letter code, i.e. GO for GameObject.
 *      
 *      Classes which are designed to test module function, and which are not architectural / intended for use in the final program, may use any name.
 * 
 * OTHER NOTES:
 * 
 *      -- INPUT PRIORITIES:    ClickBoxes are selected by LOWEST value of priority; Key Press subscribers are selected by HIGHEST value of priority.
 *      -- CAMERA SIZE:         The camera uses a size of 9. This allows textures at 20 & 40 Px/Unit to maintain consistent pixel size (180p & 360p).
 * 
 * VERSION NOTES:
 * 
 *      (0.1.c)     Additions and adjustments made to various sytems to adapt to 3D, as follows:
 *          
 *          //  GO_InputBox, GO_Word:
                --  Adjusted code to incorporate NCGF_Operations.SetAndReturnCleanedChild()
            
 *          ++  NCGF_Operations:
 *              --  Created static class Operations to contain routine multi-step functionality, such as object parenting
 * 
 *      (0.1.b)     Additions and adjustments made to various systems, as follows:
 *          
 *          ++  NCGF_PanelTool:
 *              --  Created PanelTool to aid in the production of mesh-based UI panel backgrounds in the editor
 *          
 *          //  O_ClickBox:
 *              --  Added parameter '_round'
 *          
 *          //  NCGF_UI_S_BoxKeep:
 *              --  Altered GetTopBoxAt() to exclude '_round' boxes from return value if the mouse point is outside a designated circle
 *          
 *          //  NCGF_UI_H_Actuator:
 *              --  Altered SetBoxes() so that events OE_HoverBoxStart and OE_HoverBoxEnd only fire when there exist any IDs to pass them
 *              --  In SetBoxes(), _maxBox is now removed from _curBoxes early to prevent constant firing of OE_HoverBoxEnd
 *          
 *          //  GO_Visual:
 *              --  Added Clear(), which sets the SpriteRenderer sprite to null and the SpriteRenderer color to white
 *          
 *          //  GO_MeshVisual:
 *              --  Added SetColor() to flood the mesh color with a given Color32
 *              
 *          //  NCGF_Pools:
 *              --  Added PopulatePool() to allow external scripts to populate pools up to a specified amount
 *              --  Altered Pool() to return if the passed parameter is null
 *              --  Altered Obtain() to name MonoBehaviours according to the type passed into the function
 *              --  Altered several functions to apply the same treatment to MonoBehaviours as it does to subclasses of MonoBehaviours
 *              --  Altered CleanGameObject() to set the local scale post-parenting to Vector3.one, which seems like an obvious thing to do
 *          
 *          //  GO_Button:
 *              --  Added GetID() to obtain a copy of the ID of its clickBox
 *              --  Added SetAllSprites() to set distinct sprites during runtime in a single function
 *              --  Made parameter variables public to aid in setting parameters at runtime
 *              --  Adjusted Setup() and other functions to account for if the button has been pooled, and allows it to be re-setup
 *              --  Added SetDefaultValues(), SetSoft(), and SetRound()
 *          
 *          //  GO_Word:
 *              --  Added new calls to Setup() and created bool _isSetUp to ensure properly instantiated variables
 *              
 *          //  NCGF_UI_S_Events:
 *              --  Created new event CE_CameraYoink to account for camera movement after camera update
 *              
 *          //  NCGF_Cam_H_Camera:
 *              --  Yoink() now fires event CE_CameraYoink
 *              
 *          //  NCGF_Cam_GO_Follower:
 *              --  Created OnEndCameraSetup() and subscribed to relevant events to fill in null references as necessary
 *              --  Subscribed OnEndCameraUpdate() to CE_CameraYoink to prevent occasional visual error due to yoinking
 *          
 *      (0.1.a)     First version intended for use.
 *      
 * [//]
 */
