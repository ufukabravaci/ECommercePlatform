export interface Category {
  id: string;
  name: string;
  slug: string;
  parentId: string | null;
}

export interface CategoryTreeNode extends Category {
  level: number;
  expanded: boolean;
  children?: CategoryTreeNode[];
}