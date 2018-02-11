using System;

namespace GameEngineX.Utility {
    public class Tags {

        private int tags;

        public Tags()
            : this(0) { }

        public Tags(params int[] tagIndices)
            : this(CreateTags(tagIndices)) { }

        private Tags(int tags) {
            this.tags = tags;
        }

        public void Set(params int[] tagIndices) {
            this.tags = SetTags(this.tags, tagIndices);
        }

        public void Unset(params int[] tagIndices) {
            this.tags = UnsetTags(this.tags, tagIndices);
        }

        public void AssignTag(bool set, params int[] tagIndices) {
            this.tags = AssignTags(this.tags, set, tagIndices);
        }

        public bool IsSet(int tagIndex) {
            return IsTagSet(this.tags, tagIndex);
        }

        public bool AllSet(params int[] tagIndices) {
            return AllTagsSet(this.tags, tagIndices);
        }

        public bool AnySet(params int[] tagIndices) {
            return AnyTagSet(this.tags, tagIndices);
        }

        public bool All(Tags tags) {
            return this.tags == tags.tags;
        }

        public bool Any(Tags tags) {
            return (this.tags & tags.tags) > 0;
        }

        public Tags Combine(Tags tags, Func<bool, bool, bool> combiner) {
            int newTags = CombineTags(this.tags, tags.tags, combiner);

            return new Tags(newTags);
        }

        public override string ToString() {
            return TagsToString(this.tags);
        }

        public static string TagsToString(int tags) {
            return Convert.ToString((byte)(tags & byte.MaxValue), 2).PadLeft(8, '0');
        }

        public static int CreateTags(params int[] tagIndices) {
            return SetTags(0, tagIndices);
        }

        public static int SetTags(int tags, params int[] tagIndices) {
            foreach (int tagIndex in tagIndices) {
                if (tagIndex < 0 || tagIndex > 31)
                    throw new ArgumentOutOfRangeException("tagIndex", "tagIndex must be between 0 and 31 (inclusive).");

                tags |= 1 << tagIndex;
            }


            return tags;
        }

        public static int UnsetTags(int tags, params int[] tagIndices) {
            foreach (int tagIndex in tagIndices) {
                if (tagIndex < 0 || tagIndex > 31)
                    throw new ArgumentOutOfRangeException("tagIndex", "tagIndex must be between 0 and 31 (inclusive).");

                tags &= ~(1 << tagIndex);
            }

            return tags;
        }

        public static int AssignTags(int tags, bool set, params int[] tagIndices) {
            if (set)
                return SetTags(tags, tagIndices);
            else {
                return UnsetTags(tags, tagIndices);
            }
        }

        public static bool IsTagSet(int tags, int tagIndex) {
            if (tagIndex < 0 || tagIndex > 31)
                throw new ArgumentOutOfRangeException("tagIndex", "tagIndex must be between 0 and 31 (inclusive).");

            return (tags & (1 << tagIndex)) == 1;
        }

        public static bool AllTagsSet(int tags, params int[] tagIndices) {
            foreach (int tagIndex in tagIndices) {
                if (!IsTagSet(tags, tagIndex))
                    return false;
            }

            return true;
        }

        public static bool AnyTagSet(int tags, params int[] tagIndices) {
            foreach (int tagIndex in tagIndices) {
                if (IsTagSet(tags, tagIndex))
                    return true;
            }

            return false;
        }

        public static int CombineTags(int tags1, int tags2, Func<bool, bool, bool> combinator) {
            int resultTags = 0;
            for (int i = 0; i < 32; i++) {
                resultTags = AssignTags(resultTags, combinator(IsTagSet(tags1, i), IsTagSet(tags2, i)), i);
            }

            return resultTags;
        }

    }
}
