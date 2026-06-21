import { useState, useEffect, useRef } from "react";
import { Check, ChevronDown, Plus, X, Trash2 } from "lucide-react";
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import { Textarea } from "./ui/textarea";
import { Label } from "./ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "./ui/select";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "./ui/alert-dialog";
import { useNavigate, useParams } from "react-router-dom";
import { getCategoryTree, type CategoryResponse } from "../services/categoryService";
import {
  getProductDetails,
  updateRegularProduct,
  updateWantedProduct,
  updateSwapProduct,
  deleteProduct,
  uploadOfferImages,
  uploadWantedImages,
  deleteProductImage,
  reorderProductImages,
  type ProductCondition,
  type ProductType,
  type ProductImageItem,
} from "../services/productService";
import { EGYPT_CITIES, EGYPT_COUNTRY } from "./data/locations";

const CONDITIONS: { label: string; value: ProductCondition }[] = [
  { label: "New", value: "New" },
  { label: "Like New", value: "LikeNew" },
  { label: "Used", value: "Used" },
  { label: "Broken", value: "Broken" },
];

const MAX_IMAGES = 10;

const RequiredMark = () => <span className="text-red-500">*</span>;

/**
 * Manages one group of product images (Offer or Wanted) against the live API:
 * uploads new files immediately, deletes immediately, and persists drag-reorder.
 */
function ImageManager({
  title,
  required,
  helperText,
  images,
  onUpload,
  onDelete,
  onReorder,
}: {
  title: string;
  required?: boolean;
  helperText?: string;
  images: ProductImageItem[];
  onUpload: (files: File[]) => Promise<void>;
  onDelete: (imageId: string) => Promise<void>;
  onReorder: (orderedIds: string[]) => Promise<void>;
}) {
  const [isUploading, setIsUploading] = useState(false);
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [error, setError] = useState("");
  const [draggingIndex, setDraggingIndex] = useState<number | null>(null);
  const [dragOverIndex, setDragOverIndex] = useState<number | null>(null);
  const [lightboxIndex, setLightboxIndex] = useState<number | null>(null);
  const dragIndexRef = useRef<number | null>(null);
  const hasDraggedRef = useRef(false);

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files;
    if (!files || files.length === 0) return;

    // Snapshot into a real array BEFORE resetting the input — e.target.files
    // is a live FileList tied to the DOM node, so clearing e.target.value
    // also empties this reference if read afterwards.
    const allFiles = Array.from(files);
    e.target.value = "";

    const remaining = MAX_IMAGES - images.length;
    if (remaining <= 0) return;

    const toUpload = allFiles.slice(0, remaining);
    setError("");
    setIsUploading(true);
    try {
      await onUpload(toUpload);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to upload image(s)");
    } finally {
      setIsUploading(false);
    }
  };

  const handleRemove = async (imageId: string) => {
    setError("");
    setDeletingId(imageId);
    try {
      await onDelete(imageId);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete image");
    } finally {
      setDeletingId(null);
    }
  };

  const handleDrop = async (from: number, to: number) => {
    if (from === to) return;
    const reordered = [...images];
    const [moved] = reordered.splice(from, 1);
    reordered.splice(to, 0, moved);
    setError("");
    try {
      await onReorder(reordered.map((img) => img.id));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to reorder images");
    }
  };

  return (
    <div className="bg-white rounded-2xl p-8 border border-gray-200 mb-6">
      <h3 className="text-[20px] font-semibold text-gray-900 mb-2">
        {title} {required && <RequiredMark />}
      </h3>
      {helperText && <p className="text-gray-600 text-[14px] mb-6">{helperText}</p>}

      <label
        className={`block border-2 border-dashed rounded-xl p-8 text-center transition-all duration-200 ${
          images.length >= MAX_IMAGES || isUploading
            ? "border-gray-200 cursor-not-allowed opacity-50"
            : "border-gray-300 cursor-pointer hover:border-[#4B0082] hover:bg-gray-50"
        }`}
      >
        <input
          type="file"
          accept="image/*"
          multiple
          onChange={handleFileChange}
          className="hidden"
          disabled={images.length >= MAX_IMAGES || isUploading}
        />
        <Plus className="w-12 h-12 text-gray-400 mx-auto mb-3" />
        <p className="text-gray-700 font-medium mb-1">
          {isUploading ? "Uploading..." : "Click to add photos"}
        </p>
        <p className="text-sm text-gray-500">
          PNG, JPG up to 5 MB each ({images.length}/{MAX_IMAGES})
        </p>
      </label>

      {error && <p className="text-sm text-red-600 mt-3">{error}</p>}

      {images.length > 0 && (
        <>
          <p className="text-xs text-gray-400 mt-4 mb-2">
            Drag to reorder · First image is the cover
          </p>
          <div className="grid grid-cols-5 gap-4">
            {images.map((img, i) => (
              <div
                key={img.id}
                draggable
                onDragStart={() => {
                  hasDraggedRef.current = false;
                  dragIndexRef.current = i;
                  setDraggingIndex(i);
                }}
                onDragOver={(e) => {
                  e.preventDefault();
                  if (dragOverIndex !== i) setDragOverIndex(i);
                }}
                onDrop={(e) => {
                  e.preventDefault();
                  if (dragIndexRef.current !== null && dragIndexRef.current !== i)
                    handleDrop(dragIndexRef.current, i);
                  dragIndexRef.current = null;
                  setDraggingIndex(null);
                  setDragOverIndex(null);
                }}
                onDragEnd={() => {
                  hasDraggedRef.current = true;
                  dragIndexRef.current = null;
                  setDraggingIndex(null);
                  setDragOverIndex(null);
                }}
                onClick={() => {
                  if (!hasDraggedRef.current) setLightboxIndex(i);
                  hasDraggedRef.current = false;
                }}
                className={`relative group cursor-grab active:cursor-grabbing transition-all duration-150 ${
                  draggingIndex === i ? "opacity-40 scale-95" : ""
                } ${
                  dragOverIndex === i && draggingIndex !== i
                    ? "ring-2 ring-[#4B0082] rounded-lg scale-105"
                    : ""
                } ${deletingId === img.id ? "opacity-40" : ""}`}
              >
                {i === 0 && (
                  <div className="absolute -top-2 -left-2 bg-[#4B0082] text-white text-xs font-bold px-2 py-1 rounded-md z-10">
                    Cover
                  </div>
                )}
                <img
                  src={img.url}
                  alt={`Upload ${i + 1}`}
                  draggable={false}
                  className="w-full aspect-square object-cover rounded-lg pointer-events-none select-none"
                />
                <button
                  type="button"
                  onClick={(e) => {
                    e.stopPropagation();
                    handleRemove(img.id);
                  }}
                  disabled={deletingId === img.id || images.length <= 1}
                  title={images.length <= 1 ? "Product must have at least one image" : "Remove"}
                  className="absolute -top-2 -right-2 w-7 h-7 bg-red-500 text-white rounded-full flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity duration-200 hover:bg-red-600 disabled:opacity-0"
                >
                  <X className="w-4 h-4" />
                </button>
              </div>
            ))}
          </div>
        </>
      )}

      {lightboxIndex !== null && lightboxIndex < images.length && (
        <div
          className="fixed inset-0 bg-black/80 z-50 flex items-center justify-center p-8"
          onClick={() => setLightboxIndex(null)}
        >
          {lightboxIndex > 0 && (
            <button
              onClick={(e) => {
                e.stopPropagation();
                setLightboxIndex(lightboxIndex - 1);
              }}
              className="absolute left-4 text-white text-3xl px-3 py-1 hover:bg-white/10 rounded-full"
            >
              ‹
            </button>
          )}
          <img
            src={images[lightboxIndex].url}
            alt={`Preview ${lightboxIndex + 1}`}
            className="max-w-[90vw] max-h-[85vh] object-contain rounded-lg"
            onClick={(e) => e.stopPropagation()}
          />
          {lightboxIndex < images.length - 1 && (
            <button
              onClick={(e) => {
                e.stopPropagation();
                setLightboxIndex(lightboxIndex + 1);
              }}
              className="absolute right-4 text-white text-3xl px-3 py-1 hover:bg-white/10 rounded-full"
            >
              ›
            </button>
          )}
          <div className="absolute bottom-4 text-white text-sm">
            {lightboxIndex + 1} / {images.length}
          </div>
          <button
            onClick={() => setLightboxIndex(null)}
            className="absolute top-4 right-4 text-white text-2xl px-3 py-1 hover:bg-white/10 rounded-full"
          >
            ×
          </button>
        </div>
      )}
    </div>
  );
}

export function EditProductPage() {
  const navigate = useNavigate();
  const { productId } = useParams<{ productId: string }>();

  const [productType, setProductType] = useState<ProductType>("Regular");
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showSuccess, setShowSuccess] = useState(false);
  const [error, setError] = useState("");
  const [categories, setCategories] = useState<CategoryResponse[]>([]);
  const [categoryOpen, setCategoryOpen] = useState(false);
  const [categorySearch, setCategorySearch] = useState("");
  const [cityOpen, setCityOpen] = useState(false);
  const [citySearch, setCitySearch] = useState("");
  const categoryRef = useRef<HTMLDivElement>(null);
  const cityRef = useRef<HTMLDivElement>(null);

  // Offer images (cover/primary photos) and Wanted images (Swap only)
  const [offerImages, setOfferImages] = useState<ProductImageItem[]>([]);
  const [wantedImages, setWantedImages] = useState<ProductImageItem[]>([]);

  const [formData, setFormData] = useState({
    title: "",
    description: "",
    categoryId: "",
    locationCity: "",
    condition: "" as ProductCondition | "",
    price: "",
    allowNegotiation: false,
    minPrice: "",
    maxPrice: "",
    wantedItemTitle: "",
    wantedItemDescription: "",
  });

  const selectedCategoryName =
    categories.flatMap((p) => p.subcategories).find((s) => s.id === formData.categoryId)?.name ??
    "";

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (categoryRef.current && !categoryRef.current.contains(e.target as Node))
        setCategoryOpen(false);
      if (cityRef.current && !cityRef.current.contains(e.target as Node)) setCityOpen(false);
    };
    document.addEventListener("mousedown", handler);
    return () => document.removeEventListener("mousedown", handler);
  }, []);

  useEffect(() => {
    getCategoryTree()
      .then(setCategories)
      .catch(() => {});
  }, []);

  useEffect(() => {
    if (!productId) return;
    getProductDetails(productId)
      .then((p) => {
        setProductType(p.type);
        setFormData({
          title: p.title,
          description: p.description,
          categoryId: p.categoryId,
          locationCity: p.locationCity ?? "",
          condition: CONDITIONS.find((c) => c.label === p.condition)?.value ?? "",
          price: p.price != null ? String(p.price) : "",
          allowNegotiation: p.allowNegotiation ?? false,
          minPrice: p.desiredPriceMin != null ? String(p.desiredPriceMin) : "",
          maxPrice: p.desiredPriceMax != null ? String(p.desiredPriceMax) : "",
          wantedItemTitle: p.wantedItemTitle ?? "",
          wantedItemDescription: p.wantedItemDescription ?? "",
        });
        const items = p.imageItems ?? [];
        setOfferImages(
          items
            .filter((img) => img.type === "Offer")
            .sort((a, b) => a.displayOrder - b.displayOrder)
        );
        setWantedImages(
          items
            .filter((img) => img.type === "Wanted")
            .sort((a, b) => a.displayOrder - b.displayOrder)
        );
      })
      .catch((err) => setLoadError(err instanceof Error ? err.message : "Failed to load product"))
      .finally(() => setIsLoading(false));
  }, [productId]);

  const set = (field: string, value: string | boolean) =>
    setFormData((prev) => ({ ...prev, [field]: value }));

  // ─── Image handlers (Offer images) ──────────────────────────────────────
  const handleUploadOfferImages = async (files: File[]) => {
    if (!productId) return;
    const uploaded = await uploadOfferImages(productId, files);
    setOfferImages((prev) => [
      ...prev,
      ...uploaded.map((img, idx) => ({
        id: img.id,
        url: img.url,
        type: "Offer" as const,
        displayOrder: prev.length + idx,
      })),
    ]);
  };

  const handleDeleteOfferImage = async (imageId: string) => {
    await deleteProductImage(imageId);
    setOfferImages((prev) => prev.filter((img) => img.id !== imageId));
  };

  const handleReorderOfferImages = async (orderedIds: string[]) => {
    const reordered = orderedIds.map((id, idx) => {
      const img = offerImages.find((i) => i.id === id)!;
      return { ...img, displayOrder: idx };
    });
    setOfferImages(reordered);
    await reorderProductImages(
      reordered.map((img) => ({ imageId: img.id, displayOrder: img.displayOrder }))
    );
  };

  // ─── Image handlers (Wanted images — Swap only) ─────────────────────────
  const handleUploadWantedImages = async (files: File[]) => {
    if (!productId) return;
    const uploaded = await uploadWantedImages(productId, files);
    setWantedImages((prev) => [
      ...prev,
      ...uploaded.map((img, idx) => ({
        id: img.id,
        url: img.url,
        type: "Wanted" as const,
        displayOrder: prev.length + idx,
      })),
    ]);
  };

  const handleDeleteWantedImage = async (imageId: string) => {
    await deleteProductImage(imageId);
    setWantedImages((prev) => prev.filter((img) => img.id !== imageId));
  };

  const handleReorderWantedImages = async (orderedIds: string[]) => {
    const reordered = orderedIds.map((id, idx) => {
      const img = wantedImages.find((i) => i.id === id)!;
      return { ...img, displayOrder: idx };
    });
    setWantedImages(reordered);
    await reorderProductImages(
      reordered.map((img) => ({ imageId: img.id, displayOrder: img.displayOrder }))
    );
  };

  const handleDeleteProduct = async () => {
    if (!productId) return;
    try {
      await deleteProduct(productId);
      navigate("/my-products");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete product");
    }
  };

  const validate = (): string => {
    if (!formData.title.trim()) return "Please enter a title";
    if (!formData.description.trim()) return "Please enter a description";
    if (!formData.categoryId) return "Please select a category";
    if (!formData.condition) return "Please select a condition";
    if (!formData.locationCity) return "Please select a city";
    if (productType === "Regular" && !formData.price) return "Please enter a price";
    if (productType === "Wanted" && (!formData.minPrice || !formData.maxPrice))
      return "Please enter a price range";
    if (productType === "Wanted" && parseFloat(formData.maxPrice) < parseFloat(formData.minPrice))
      return "Maximum price must be greater than or equal to minimum price";
    if (productType === "Swap" && (!formData.wantedItemTitle || !formData.wantedItemDescription))
      return "Please fill in both swap fields";
    return "";
  };

  const handleSubmit = async () => {
    if (!productId) return;
    const validationError = validate();
    if (validationError) {
      setError(validationError);
      return;
    }
    setError("");
    setIsSubmitting(true);
    try {
      const basicInfo = {
        title: formData.title,
        description: formData.description,
        categoryId: formData.categoryId,
        condition: formData.condition as ProductCondition,
        locationCity: formData.locationCity || undefined,
        locationCountry: formData.locationCity ? EGYPT_COUNTRY : undefined,
      };
      if (productType === "Regular") {
        await updateRegularProduct(productId, {
          basicInfo,
          price: parseFloat(formData.price),
          allowNegotiation: formData.allowNegotiation,
        });
      } else if (productType === "Wanted") {
        await updateWantedProduct(productId, {
          basicInfo,
          desiredPriceMin: parseFloat(formData.minPrice),
          desiredPriceMax: parseFloat(formData.maxPrice),
        });
      } else {
        await updateSwapProduct(productId, {
          basicInfo,
          wantedItemTitle: formData.wantedItemTitle,
          wantedItemDescription: formData.wantedItemDescription,
        });
      }
      setShowSuccess(true);
      setTimeout(() => navigate(`/product/${productId}`), 1500);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Something went wrong");
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center px-8">
        <div className="w-10 h-10 border-2 border-[#4B0082] border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  if (loadError) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center px-8">
        <div className="bg-white rounded-2xl shadow-lg border border-gray-200 p-12 max-w-md text-center">
          <h2 className="text-[24px] font-bold text-gray-900 mb-3">Unable to load product</h2>
          <p className="text-gray-600 text-[16px] mb-6">{loadError}</p>
          <Button onClick={() => navigate(-1)}>Go Back</Button>
        </div>
      </div>
    );
  }

  if (showSuccess) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center px-8">
        <div className="bg-white rounded-2xl shadow-lg border border-gray-200 p-12 max-w-md text-center">
          <div className="w-20 h-20 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-6">
            <Check className="w-10 h-10 text-green-600" />
          </div>
          <h2 className="text-[28px] font-bold text-gray-900 mb-3">Changes Saved!</h2>
          <p className="text-gray-600 text-[16px]">Your listing has been successfully updated.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-[900px] mx-auto px-8 py-12">
        {/* Header */}
        <div className="mb-8">
          <button
            onClick={() => navigate(-1)}
            className="text-[#4B0082] hover:underline mb-4 flex items-center gap-2"
          >
            ← Back
          </button>
          <div className="flex items-start justify-between gap-4">
            <div>
              <h1 className="text-[36px] font-bold text-gray-900 mb-2">Edit Your Item</h1>
              <p className="text-gray-600 text-[16px]">Update the details of your listing</p>
              <p className="text-gray-500 text-[14px] mt-2">
                All fields marked with <RequiredMark /> are required.
              </p>
            </div>
            <AlertDialog>
              <AlertDialogTrigger asChild>
                <Button
                  variant="destructive"
                  size="lg"
                  className="flex items-center gap-2 shrink-0"
                >
                  <Trash2 className="w-4 h-4" />
                  Delete Product
                </Button>
              </AlertDialogTrigger>
              <AlertDialogContent>
                <AlertDialogHeader>
                  <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
                  <AlertDialogDescription>
                    This action cannot be undone. This will permanently delete your product listing
                    and remove all associated data.
                  </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                  <AlertDialogCancel>Cancel</AlertDialogCancel>
                  <AlertDialogAction
                    onClick={handleDeleteProduct}
                    className="bg-red-600 hover:bg-red-700"
                  >
                    Delete
                  </AlertDialogAction>
                </AlertDialogFooter>
              </AlertDialogContent>
            </AlertDialog>
          </div>
        </div>

        {/* Product Type (read-only) */}
        <div className="bg-white rounded-2xl p-8 border border-gray-200 mb-6">
          <h3 className="text-[20px] font-semibold text-gray-900 mb-4">Product Type</h3>
          <div className="flex items-center w-full border border-gray-200 rounded-md px-3 h-9 text-sm text-gray-500 bg-gray-50">
            {productType === "Regular" ? "Regular Sale" : productType}
          </div>
          <p className="text-sm text-gray-500 mt-3">Product type cannot be changed.</p>
        </div>

        {/* Basic Info */}
        <div className="bg-white rounded-2xl p-8 border border-gray-200 mb-6">
          <h3 className="text-[20px] font-semibold text-gray-900 mb-6">Basic Information</h3>
          <div className="space-y-6">
            <div>
              <Label htmlFor="title">
                Title <RequiredMark />
              </Label>
              <Input
                id="title"
                placeholder="e.g., Vintage Polaroid Camera SX-70"
                value={formData.title}
                onChange={(e) => set("title", e.target.value)}
                className="mt-2"
              />
            </div>
            <div>
              <Label htmlFor="description">
                Description <RequiredMark />
              </Label>
              <Textarea
                id="description"
                placeholder="Describe your item in detail..."
                value={formData.description}
                onChange={(e) => set("description", e.target.value)}
                maxLength={1000}
                className="mt-2 min-h-[150px]"
              />
              <p className="text-sm text-gray-500 mt-2">
                {formData.description.length} / 1000 characters
              </p>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label>
                  Category <RequiredMark />
                </Label>
                <div className="relative mt-2" ref={categoryRef}>
                  <div
                    onClick={() => {
                      setCategoryOpen(true);
                      setCategorySearch("");
                    }}
                    className={`flex items-center justify-between w-full border rounded-md px-3 h-9 text-sm cursor-text transition-[color,box-shadow] ${
                      categoryOpen
                        ? "border-[#4B0082] ring-[3px] ring-[#4B0082]/20"
                        : "border-gray-300"
                    }`}
                  >
                    {categoryOpen ? (
                      <input
                        autoFocus
                        value={categorySearch}
                        onChange={(e) => setCategorySearch(e.target.value)}
                        placeholder="Search categories..."
                        className="flex-1 outline-none bg-transparent text-sm"
                      />
                    ) : (
                      <span
                        className={selectedCategoryName ? "text-gray-900" : "text-muted-foreground"}
                      >
                        {selectedCategoryName || "Select category"}
                      </span>
                    )}
                    <ChevronDown className="w-4 h-4 text-gray-400 flex-shrink-0" />
                  </div>
                  {categoryOpen && (
                    <div className="absolute z-50 w-full mt-1 bg-white border border-gray-200 rounded-md shadow-md max-h-60 overflow-y-auto">
                      {(() => {
                        const filtered = categories
                          .map((parent) => ({
                            ...parent,
                            matchingSubs: parent.subcategories.filter((sub) =>
                              sub.name.toLowerCase().includes(categorySearch.toLowerCase())
                            ),
                          }))
                          .filter((parent) => parent.matchingSubs.length > 0);
                        return filtered.length === 0 ? (
                          <p className="text-sm text-gray-400 text-center py-4">
                            No categories found
                          </p>
                        ) : (
                          filtered.map((parent) => (
                            <div key={parent.id}>
                              <p className="text-xs text-gray-400 font-semibold px-3 pt-3 pb-1">
                                {parent.name}
                              </p>
                              {parent.matchingSubs.map((sub) => (
                                <button
                                  key={sub.id}
                                  type="button"
                                  onClick={() => {
                                    set("categoryId", sub.id);
                                    setCategoryOpen(false);
                                    setCategorySearch("");
                                  }}
                                  className={`w-full text-left px-4 py-2 text-sm hover:bg-gray-50 ${
                                    formData.categoryId === sub.id
                                      ? "text-[#4B0082] font-medium bg-purple-50"
                                      : "text-gray-900"
                                  }`}
                                >
                                  {sub.name}
                                </button>
                              ))}
                            </div>
                          ))
                        );
                      })()}
                    </div>
                  )}
                </div>
              </div>
              <div>
                <Label>
                  Condition <RequiredMark />
                </Label>
                <Select value={formData.condition} onValueChange={(v) => set("condition", v)}>
                  <SelectTrigger className="mt-2">
                    <SelectValue placeholder="Select condition" />
                  </SelectTrigger>
                  <SelectContent>
                    {CONDITIONS.map((c) => (
                      <SelectItem key={c.value} value={c.value}>
                        {c.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>
          </div>
        </div>

        {/* Location */}
        <div className="bg-white rounded-2xl p-8 border border-gray-200 mb-6">
          <h3 className="text-[20px] font-semibold text-gray-900 mb-6">Location</h3>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <Label>
                City <RequiredMark />
              </Label>
              <div className="relative mt-2" ref={cityRef}>
                <div
                  onClick={() => {
                    setCityOpen(true);
                    setCitySearch("");
                  }}
                  className={`flex items-center justify-between w-full border rounded-md px-3 h-9 text-sm cursor-text transition-[color,box-shadow] ${
                    cityOpen ? "border-[#4B0082] ring-[3px] ring-[#4B0082]/20" : "border-gray-300"
                  }`}
                >
                  {cityOpen ? (
                    <input
                      autoFocus
                      value={citySearch}
                      onChange={(e) => setCitySearch(e.target.value)}
                      placeholder="Search cities..."
                      className="flex-1 outline-none bg-transparent text-sm"
                    />
                  ) : (
                    <span
                      className={formData.locationCity ? "text-gray-900" : "text-muted-foreground"}
                    >
                      {formData.locationCity || "Select city"}
                    </span>
                  )}
                  <ChevronDown className="w-4 h-4 text-gray-400 flex-shrink-0" />
                </div>
                {cityOpen && (
                  <div className="absolute z-50 w-full mt-1 bg-white border border-gray-200 rounded-md shadow-md max-h-60 overflow-y-auto">
                    {(() => {
                      const filtered = EGYPT_CITIES.filter((city) =>
                        city.toLowerCase().includes(citySearch.toLowerCase())
                      );
                      return filtered.length === 0 ? (
                        <p className="text-sm text-gray-400 text-center py-4">No cities found</p>
                      ) : (
                        filtered.map((city) => (
                          <button
                            key={city}
                            type="button"
                            onClick={() => {
                              set("locationCity", city);
                              setCityOpen(false);
                              setCitySearch("");
                            }}
                            className={`w-full text-left px-4 py-2 text-sm hover:bg-gray-50 ${
                              formData.locationCity === city
                                ? "text-[#4B0082] font-medium bg-purple-50"
                                : "text-gray-900"
                            }`}
                          >
                            {city}
                          </button>
                        ))
                      );
                    })()}
                  </div>
                )}
              </div>
            </div>
            <div>
              <Label>Country</Label>
              <div className="flex items-center w-full border border-gray-200 rounded-md px-3 h-9 mt-2 text-sm text-gray-500 bg-gray-50">
                {EGYPT_COUNTRY}
              </div>
            </div>
          </div>
        </div>

        {/* Pricing / Swap Details */}
        <div className="bg-white rounded-2xl p-8 border border-gray-200 mb-8">
          <h3 className="text-[20px] font-semibold text-gray-900 mb-6">
            {productType === "Regular" && "Pricing"}
            {productType === "Wanted" && "Desired Price Range"}
            {productType === "Swap" && "Swap Details"}
          </h3>

          {productType === "Regular" && (
            <div className="space-y-4">
              <div>
                <Label htmlFor="price">
                  Price <RequiredMark />
                </Label>
                <div className="relative mt-2">
                  <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-500 font-medium">
                    $
                  </span>
                  <Input
                    id="price"
                    type="number"
                    min="0"
                    placeholder="0.00"
                    value={formData.price}
                    onChange={(e) => set("price", e.target.value)}
                    className="pl-8"
                  />
                </div>
              </div>
              <div className="flex items-center gap-3">
                <input
                  type="checkbox"
                  id="allowNegotiation"
                  checked={formData.allowNegotiation}
                  onChange={(e) => set("allowNegotiation", e.target.checked)}
                  className="w-4 h-4 accent-[#4B0082]"
                />
                <Label htmlFor="allowNegotiation" className="cursor-pointer">
                  Allow negotiation
                </Label>
              </div>
            </div>
          )}

          {productType === "Wanted" && (
            <div className="grid grid-cols-2 gap-6">
              <div>
                <Label htmlFor="minPrice">
                  Minimum Price <RequiredMark />
                </Label>
                <div className="relative mt-2">
                  <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-500 font-medium">
                    $
                  </span>
                  <Input
                    id="minPrice"
                    type="number"
                    min="0"
                    placeholder="0.00"
                    value={formData.minPrice}
                    onChange={(e) => set("minPrice", e.target.value)}
                    className="pl-8"
                  />
                </div>
              </div>
              <div>
                <Label htmlFor="maxPrice">
                  Maximum Price <RequiredMark />
                </Label>
                <div className="relative mt-2">
                  <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-500 font-medium">
                    $
                  </span>
                  <Input
                    id="maxPrice"
                    type="number"
                    min="0"
                    placeholder="0.00"
                    value={formData.maxPrice}
                    onChange={(e) => set("maxPrice", e.target.value)}
                    className="pl-8"
                  />
                </div>
              </div>
            </div>
          )}

          {productType === "Swap" && (
            <div className="space-y-6">
              <div>
                <Label htmlFor="wantedItemTitle">
                  I'm Offering — Title <RequiredMark />
                </Label>
                <Input
                  id="wantedItemTitle"
                  placeholder="e.g., MacBook Pro 2020"
                  value={formData.wantedItemTitle}
                  onChange={(e) => set("wantedItemTitle", e.target.value)}
                  className="mt-2"
                />
              </div>
              <div>
                <Label htmlFor="wantedItemDescription">
                  I'm Looking For — Description <RequiredMark />
                </Label>
                <Textarea
                  id="wantedItemDescription"
                  placeholder="Describe what you want in return..."
                  value={formData.wantedItemDescription}
                  onChange={(e) => set("wantedItemDescription", e.target.value)}
                  className="mt-2 min-h-[100px]"
                />
              </div>
            </div>
          )}
        </div>

        {/* Photos */}
        <ImageManager
          title={productType === "Swap" ? "Photos of Your Item" : "Photos"}
          required
          helperText="Add at least 1 photo. Max 10 images, up to 5 MB each. Drag to reorder, click to preview."
          images={offerImages}
          onUpload={handleUploadOfferImages}
          onDelete={handleDeleteOfferImage}
          onReorder={handleReorderOfferImages}
        />

        {/* Wanted Images (Swap only) */}
        {productType === "Swap" && (
          <ImageManager
            title="Photos of What You Want"
            helperText="Optional. Add reference photos of the item you want in return. Max 10 images, up to 5 MB each."
            images={wantedImages}
            onUpload={handleUploadWantedImages}
            onDelete={handleDeleteWantedImage}
            onReorder={handleReorderWantedImages}
          />
        )}

        {/* Error */}
        {error && (
          <div className="mb-4 px-4 py-3 rounded-xl bg-red-50 border border-red-200 text-red-700 text-sm">
            {error}
          </div>
        )}

        {/* Actions */}
        <div className="flex gap-4">
          <Button
            variant="outline"
            size="lg"
            onClick={() => navigate(-1)}
            className="flex-1"
            disabled={isSubmitting}
          >
            Cancel
          </Button>
          <Button
            size="lg"
            onClick={handleSubmit}
            disabled={isSubmitting}
            className="flex-1 bg-gradient-to-r from-[#4B0082] to-[#3d2e7c] hover:opacity-90"
          >
            {isSubmitting ? (
              <>
                <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin mr-2" />
                Saving...
              </>
            ) : (
              "Save Changes"
            )}
          </Button>
        </div>
      </div>
    </div>
  );
}
