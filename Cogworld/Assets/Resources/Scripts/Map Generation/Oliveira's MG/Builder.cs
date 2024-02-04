using System;
using UnityEngine;

public abstract class Builder : IEquatable<Builder> {
        private static int instances = 0;
        private int id;
        protected DungeonGeneratorCTR map;
        protected Vector2Int location;
        protected Vector2Int forward;
        protected int age;
        protected int maxAge;
        protected int generation;

        public Builder(ref DungeonGeneratorCTR _map, Vector2Int _location, Vector2Int _forward, int _age, int _maxAge, int _generation) {
            map = _map;
            location = _location;
            forward = _forward;
            age = _age;
            maxAge = _maxAge;
            generation = _generation;
            instances++;
            id = instances;
        }

        public int Age {get => age;}
        public int MaxAge {get => maxAge;}
        public int Generation {get => generation;}
        public int ID {get => id;}
        public Vector2Int Location {get => location;}
        public Vector2Int Forward {get => forward;}
        public void AdvanceAge(int adv) {age = age + adv;}
        public abstract bool StepAhead();

        public string Stats() {
            return "pos = " + location + " forward " + forward + " generation = " + generation + " age = " + age;
        }

        public bool Equals(Builder other) {
            if (other == null) 
                return false;
            if (this.id == other.ID)
                return true;
            else 
                return false;
        }

        public override bool Equals(object other) {
            if (other == null)
                return false;

            Builder builder = other as Builder;
            if (builder == null)
                return false;
            else 
                return Equals(builder);
        }

        public override int GetHashCode() {
            return this.ID.GetHashCode();
        }
    }
