import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useListingPhotos } from "../../hooks/useListingPhotos";
import "./ListingGallery.css";

type ListingGalleryProps = {
  listingId: string;
  fallbackSrc?: string;
  fallbackAlt?: string;
};

const ListingGallery = ({ listingId, fallbackSrc, fallbackAlt }: ListingGalleryProps) => {
  const { t } = useTranslation();
  const { photos, isLoading } = useListingPhotos(listingId);
  const [activeIndex, setActiveIndex] = useState(0);

  const visiblePhotos = photos.filter((photo) => photo.url);

  if (isLoading) {
    return <div className="listing-detail-hero skeleton">{t("listingDetail.photosLoading")}</div>;
  }

  if (visiblePhotos.length === 0) {
    if (!fallbackSrc) return null;
    return (
      <div className="listing-detail-hero">
        <img src={fallbackSrc} alt={fallbackAlt ?? t("listingDetail.photoAlt")} />
      </div>
    );
  }

  const safeIndex = Math.min(activeIndex, visiblePhotos.length - 1);
  const next = () => setActiveIndex((prev) => (prev + 1) % visiblePhotos.length);
  const prev = () =>
    setActiveIndex((prev) => (prev - 1 + visiblePhotos.length) % visiblePhotos.length);

  return (
    <div className="listing-detail-hero">
      <img src={visiblePhotos[safeIndex].url} alt={fallbackAlt ?? t("listingDetail.photoAlt")} />

      {visiblePhotos.length > 1 && (
        <>
          <button type="button" className="nav-btn prev" onClick={prev} aria-label={t("listingDetail.photoPrev")}>
            ❮
          </button>
          <button type="button" className="nav-btn next" onClick={next} aria-label={t("listingDetail.photoNext")}>
            ❯
          </button>
          <div className="counter">
            {safeIndex + 1} / {visiblePhotos.length}
          </div>
        </>
      )}
    </div>
  );
};

export default ListingGallery;
