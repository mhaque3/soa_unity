using System.Collections;
using soa;

namespace soa{
    public class Optional<T>{
        private bool valueSet;
        private T value;

        public Optional()
        {
            valueSet = false;
        }

        public Optional(T value){
            valueSet = true;
            this.value = value;
        }

        public bool GetIsSet(){
            return valueSet;
        }

        public T GetValue(){
            return value;
        }

        public override string ToString()
        {
            return GetValue().ToString();
        }
    }
}

