namespace GameEngineX.Game.GameObjects.GameObjectComponents {
    public abstract class GameObjectComponent {
        public GameObject GameObject { get; internal set; }

        public bool IsEnabled;

        protected GameObjectComponent() {
            IsEnabled = true;
        }

        public void Destroy() {
            GameObject.RemoveComponent(this);
        }

        public virtual void Initialize() { }

        public virtual void Death() { }

        public virtual void Update(float deltaTime) { }

        public Transform Transform => GameObject.Transform;

        //internal XMLElement Serialize() {
        //    XMLElement root = new XMLElement("GameObjectComponent");

        //    root.AddDataElement("Type", this.GetType().FullName);
        //    root.AddDataElement("IsEnabled", IsEnabled.ToString());

        //    XMLElement fieldDataElement = new XMLElement("FieldData");
        //    root.AddElement(fieldDataElement);

        //    IEnumerable<FieldInfo> serializableFields = GetType().GetFields().Where(f => Attribute.IsDefined(f, typeof(SerializationUtils.Serializable)));

        //    foreach (FieldInfo field in serializableFields) {
        //        object fieldValue = field.GetValue(this);

        //        string dataString = SerializationUtils.Serialize(fieldValue).Single();

        //        fieldDataElement.AddDataElement(field.Name, dataString);
        //    }

        //    return root;
        //}

        //internal static void Deserialize(XMLElement dataElement, GameObject attachedGameObject) {
        //    (Type type, bool isEnabled, IEnumerable<KeyValuePair<string, object>> fields) componentData = GameObjectComponent.DeserializeData(dataElement);

        //    attachedGameObject.AddComponent(componentData.type, componentData.isEnabled, componentData.fields);
        //}

        //internal static (Type type, bool isEnabled, IEnumerable<KeyValuePair<string, object>> fields) DeserializeData(XMLElement e) {
        //    if (!e.HasElement("Type") || !e.GetElement("Type").HasData)
        //        throw new SerializationException("Cannot deserialize game object component.");

        //    Type t = Type.GetType(e.GetElement("Type").Data);

        //    Dictionary<string, object> fieldData = new Dictionary<string, object>();
        //    if (!e.GetElement("IsEnabled").HasData || !bool.TryParse(e.GetElement("IsEnabled").Data, out var isEnabled))
        //        throw new SerializationException("Cannot deserialize game object component.");

        //    XMLElement fieldsElement = e.GetElement("FieldData");
        //    if (fieldsElement.HasData)
        //        throw new SerializationException("Cannot deserialize game object component.");

        //    foreach (XMLElement fieldElement in fieldsElement.NestedElements) {
        //        if (!fieldElement.HasData || !fieldElement.HasAttribute("Name"))
        //            throw new SerializationException("Cannot deserialize game object component.");

        //        fieldData[fieldElement.GetAttribute("Name")] = SerializationUtils.Deserialize(fieldElement.Data).Single();
        //    }

        //    return (t, isEnabled, fieldData);
        //}
    }
}
