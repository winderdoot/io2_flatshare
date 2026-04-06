import { useTranslation } from "react-i18next";

export const ColorPalette = () => {
  const { t } = useTranslation();
  return (
    <>
      <h1>{t("colorPalette.title")}</h1>
      <section className='cards'>
        <div className='card color1'></div>
        <div className='card color2'></div>
        <div className='card color3'></div>
        <div className='card color4'></div>
        <div className='card color5'></div>
        <div className='card color6'></div>
      </section>
    </>
  )
};